using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ManpowerAllocation
{
    public class ESS_ManpowerAllocation_ManpowerRatesController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_ManpowerAllocation_ManpowerRates
        #region _ManpowerRates
        public ActionResult ESS_ManpowerAllocation_ManpowerRates(string ManpowerRateId_Encrypted, int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 280;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                KPI_ManpowerRate KPIManpowerRate = new KPI_ManpowerRate();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;

                DynamicParameters paramCategory = new DynamicParameters();
                paramCategory.Add("@query", "   Select KPICategoryId as Id ,KPICategoryName as Name from KPI_Category where Deactivate=0 order by Name");
                var GetKPICategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory).ToList();
                ViewBag.KPICategoryName = GetKPICategoryName;
                ViewBag.GetBranchName = "";
                ViewBag.GetSubCategoryName = "";

                DynamicParameters Rates = new DynamicParameters();
                Rates.Add("@p_ManpowerRateId_Encrypted", "List");
               var  GETKPIManpowerRate = DapperORM.DynamicList("sp_List_KPI_ManpowerRate", Rates);
                var GetManpowerRateBranchId = KPIManpowerRate.ManpowerRateBranchId;
                //TempData["FromDate"] = KPIManpowerRate.ManpowerRateFromDate;
                ///TempData["ToDate"] = KPIManpowerRate.ManpowerRateToDate;
                if(GETKPIManpowerRate != null)
                {
                    ViewBag.GetManPowerRatesList = GETKPIManpowerRate;
                }
                else
                {
                    ViewBag.GetManPowerRatesList = "";
                }

                //if (ManpowerRateId_Encrypted != null)
                //{
                //    ViewBag.AddUpdateTitle = "Update";
                    

                //    DynamicParameters paramBranch = new DynamicParameters();
                //    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                //    paramBranch.Add("@p_CmpId", CmpId);
                //    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                //    ViewBag.GetBranchName = data;

                //    DynamicParameters paramCategoryBR = new DynamicParameters();
                //    paramCategoryBR.Add("@query", "Select KPISubCategoryId as Id ,KPISubCategoryFName as Name from KPI_subCategory where KPI_subCategory.Deactivate=0 and KPISubCategoryBranchId=" + GetManpowerRateBranchId + " order by Name;");
                //    var GetGetManpowerRateSub = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategoryBR).ToList();
                //    ViewBag.GetSubCategoryName = GetGetManpowerRateSub;
                //}
                return View(KPIManpowerRate);
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

        #region IsValidation
        [HttpGet]
        public ActionResult IsManpowerRatesExists(string ManpowerRateId_Encrypted, DateTime ManpowerRateFromDate, DateTime ManpowerRateToDate, double ManpowerRateKPICategoryId, string ManpowerRate,double ManpowerRateKPISubCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var GetManpowerRateFromDate = ManpowerRateFromDate.ToString("yyyy-MM-dd");
                var GetManpowerRateToDate = ManpowerRateToDate.ToString("yyyy-MM-dd");
                param.Add("@p_process", "IsValidation");
                param.Add("@p_ManpowerRateId_Encrypted", ManpowerRateId_Encrypted);
                param.Add("@p_ManpowerRateFromDate", GetManpowerRateFromDate);
                param.Add("@p_ManpowerRateToDate", GetManpowerRateToDate);
                param.Add("@p_ManpowerRateKPICategoryId", ManpowerRateKPICategoryId);
                param.Add("@p_ManpowerRateKPISubCategoryId", ManpowerRateKPISubCategoryId);
                
                param.Add("@p_ManpowerRate", ManpowerRate);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_KPI_ManpowerRate", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                if (Message != "")
                {
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
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
        public ActionResult SaveUpdate(KPI_ManpowerRate ObjKPIManpowerRate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(ObjKPIManpowerRate.ManpowerRateId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ManpowerRateId_Encrypted", ObjKPIManpowerRate.ManpowerRateId_Encrypted);
                param.Add("@p_CmpID", ObjKPIManpowerRate.CmpID);
                param.Add("@p_ManpowerRateBranchId", ObjKPIManpowerRate.ManpowerRateBranchId);
                param.Add("@p_ManpowerRateFromDate", ObjKPIManpowerRate.ManpowerRateFromDate);
                param.Add("@p_ManpowerRateToDate", ObjKPIManpowerRate.ManpowerRateToDate);
                param.Add("@p_ManpowerRateKPICategoryId", ObjKPIManpowerRate.ManpowerRateKPICategoryId);
                param.Add("@p_ManpowerRateKPISubCategoryId", ObjKPIManpowerRate.ManpowerRateKPISubCategoryId);
                param.Add("@p_ManpowerRate", ObjKPIManpowerRate.ManpowerRate);
                param.Add("@p_ManpowerRateHeadCount", ObjKPIManpowerRate.ManpowerRateHeadCount);
                param.Add("@p_ManpowerRateManDays", ObjKPIManpowerRate.ManpowerRateManDays);
                param.Add("@p_ManpowerRateMonth", ObjKPIManpowerRate.ManpowerRateMonth);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_KPI_ManpowerRate", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("ESS_ManpowerAllocation_ManpowerRates", "ESS_ManpowerAllocation_ManpowerRates");
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
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 280;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_ManpowerRateId_Encrypted", "List");
                param.Add("@P_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.DynamicList("sp_List_KPI_ManpowerRate", param);
                ViewBag.GetManPowerRatesList = data;
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
        public ActionResult Delete(string ManpowerRateId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ManpowerRateId_Encrypted", ManpowerRateId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_KPI_ManpowerRate", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_ManpowerAllocation_ManpowerRates", "ESS_ManpowerAllocation_ManpowerRates");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetSubCategory
        [HttpGet]
        public ActionResult GetSubCategory(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select KPISubCategoryId as Id ,KPISubCategoryFName as Name from KPI_subCategory where KPI_subCategory.Deactivate=0 and KPISubCategoryBranchId="+ BranchId + " order by Name;");
                var GetSubCategoryList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@query", "Select KPI_ManpowerRate.ManpowerRateId_Encrypted,KPI_Category.KPICategoryName,KPI_subCategory.KPISubCategoryFName,replace(convert(nvarchar(12),KPI_ManpowerRate.ManpowerRateFromDate,106),' ','/') as ManpowerRateFromDate,replace(convert(nvarchar(12),KPI_ManpowerRate.ManpowerRateToDate,106),' ','/') as ManpowerRateFromDate,KPI_ManpowerRate.ManpowerRate from KPI_ManpowerRate,KPI_subCategory,KPI_Category WHERE KPI_ManpowerRate.Deactivate=0 and KPI_subCategory.Deactivate=0 and  KPI_Category.Deactivate=0 and KPI_ManpowerRate.ManpowerRateKPICategoryId=KPI_Category.KPICategoryId and KPI_ManpowerRate.ManpowerRateKPISubCategoryId=KPI_subCategory.KPISubCategoryId  and KPI_ManpowerRate.ManpowerRateBranchId=" + BranchId + "");
                var ManpowerRateList = DapperORM.DynamicList("sp_QueryExcution", paramList);

                return Json(new { GetSubCategoryList= GetSubCategoryList, ManpowerRateList=ManpowerRateList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetSubCategory
        [HttpGet]
        public ActionResult GetRates(int BranchId,string Rates,int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@query", "Select KPI_Category.KPICategoryName,KPI_subCategory.KPISubCategoryFName,replace(convert(nvarchar(12),KPI_ManpowerRate.ManpowerRateFromDate,106),' ','/') as ManpowerRateFromDate,replace(convert(nvarchar(12),KPI_ManpowerRate.ManpowerRateToDate,106),' ','/') as ManpowerRateFromDate,KPI_ManpowerRate.ManpowerRate from KPI_ManpowerRate,KPI_subCategory,KPI_Category WHERE KPI_ManpowerRate.Deactivate=0 and KPI_subCategory.Deactivate=0 and  KPI_Category.Deactivate=0 and KPI_ManpowerRate.ManpowerRateKPICategoryId=KPI_Category.KPICategoryId and KPI_ManpowerRate.ManpowerRateKPISubCategoryId=KPI_subCategory.KPISubCategoryId and KPI_ManpowerRate.ManpowerRate='"+ Rates + "' AND KPI_ManpowerRate.CmpId="+ CmpId + " and KPI_ManpowerRate.ManpowerRateBranchId="+BranchId+"");
                var data = DapperORM.DynamicList("sp_QueryExcution", paramList);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion



        public ActionResult DynamicExportToExcel()
        {
            // Create a new workbook and worksheet
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            // Sample data for demonstration
            //for (int i = 1; i <= 500; i++)
            //{
            //    worksheet.Cell(i, 1).Value = $"Row {i}";
            //    worksheet.Cell(i, 2).Value = $"Row {i}";
            //    worksheet.Cell(i, 3).Value = $"Data {i}";
            //}

            // Merge every 7 rows in the first column
            DataTable dt = new DataTable();
            List<dynamic> data = new List<dynamic>();
            param.Add("@Fromdate", "2024-04-01");
            param.Add("@Todate", "2024-04-30");
            param.Add("@p_Branhid", Session["BranchId"]);
            data = DapperORM.ExecuteSP<dynamic>("Rpt_Monthlyworkduration", param).ToList();
            dt = ConvertToDataTable(data);
            dt.Rows.RemoveAt(0);
            worksheet.Cell(1, 1).InsertTable(dt);

            int totalRows = worksheet.RowsUsed().Count();

            var colors = new[] { XLColor.LightGray, XLColor.LightGray };
            int colorIndex = 0;

            int Loop = totalRows;
            Loop = Loop / 12;
            int ao = 2;
            int ab = 0;

            for (int m = 0; m < Loop; m++)
            {
                ab = ao + 10;
                worksheet.Range(ao, 1, ab, 1).Merge();
                worksheet.Range(ao, 2, ab, 2).Merge();

                var groupRange = worksheet.Range(ao - 1, 1, ao - 1, worksheet.LastColumnUsed().ColumnNumber());
                groupRange.Style.Fill.BackgroundColor = colors[colorIndex % colors.Length];
                colorIndex++;
                ao = ao + 12;
            }

            //var firstRowValuesForPaste = worksheet.FirstRowUsed();
            //var firstRowRange = worksheet.Range(firstRowValuesForPaste.FirstCellUsed(), firstRowValuesForPaste.LastCellUsed());
            //int o = 0;
            //var groups = Enumerable.Range(2, totalRows - 1)
            //                    .Select((value, index) => new { value, index })
            //                    .GroupBy(x => x.index / 12)
            //                    .Select(g => g.Select(x => x.value));


            //foreach (var group in groups)
            //{
            //    var firstRow = group.First();
            //    var lastRow = group.Last();
            //    worksheet.Range(firstRow, 1, lastRow, 1).Merge();
            //    worksheet.Range(firstRow, 2, lastRow, 2).Merge();
            //}
            var column = worksheet.Column(2);
            column.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            column.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            column.AdjustToContents();

            var usedRange = worksheet.RangeUsed();
            usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Font.FontName = "Calibri"; // Change the font name
            usedRange.Style.Font.FontSize = 10;

            // Save the workbook
            workbook.SaveAs("D:\\TestMyExcel.xlsx");
            TempData["Title"] = "D:\\TestMyExcel.xlsx";
            TempData["Message"] = "File Download Successfull";
            TempData["Icon"] = "success";
            return RedirectToAction("GetList", "ESS_ManpowerAllocation_ManpowerRates", new { Area = "ESS" });
        }


        private static DataTable ConvertToDataTable(IEnumerable<dynamic> data)
        {
            var dataTable = new DataTable();
            var firstRecord = data.FirstOrDefault() as IDictionary<string, object>;

            if (firstRecord != null)
            {
                foreach (var kvp in firstRecord)
                {
                    dataTable.Columns.Add(kvp.Key, kvp.Value == null ? typeof(object) : kvp.Value.GetType());
                }

                foreach (var record in data)
                {
                    var dataRow = dataTable.NewRow();
                    foreach (var kvp in record)
                    {
                        dataRow[kvp.Key] = kvp.Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }
    }
}