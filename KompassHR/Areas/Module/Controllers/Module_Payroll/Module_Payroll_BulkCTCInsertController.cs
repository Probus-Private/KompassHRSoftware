using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_BulkCTCInsertController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        DataTable dt_company = new DataTable();
        DataTable dt_companyName = new DataTable();
        DataTable dt_BU = new DataTable();
        DataTable dt_CategoryName = new DataTable();
        DataTable dt_SalaryBankTranfer = new DataTable();
        DataTable dt_BudgetName = new DataTable();
        DataTable dt_CheckEmployeeNo = new DataTable();
        // GET: Module/Module_Payroll_BulkCTCInsert
        #region Module_Payroll_BulkCTCInsert
        public ActionResult Module_Payroll_BulkCTCInsert(Payroll_BulkCTCInsert ObjBulkCTCInsert)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 543;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                ViewBag.CompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();

                //if (ObjBulkCTCInsert.CmpId != 0)
                //{
                //    DynamicParameters paramy = new DynamicParameters();
                //    paramy.Add("@p_employeeid", ObjBulkCTCInsert.CmpId);
                //    ViewBag.CompanyName = DapperORM.ReturnList<dynamic>("sp_Template_BulkInsert_CTC", paramy).ToList();
                //}
                ViewBag.ExcelData = null;

                var results = DapperORM.DynamicQueryMultiple(@"Select CategoryId as Id,CategoryName as Name from Payroll_CTC_Category where Deactivate=0");
                ViewBag.CTC_Category = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.BranchName = "";
                ViewBag.BudgetName = "";
                ViewBag.SalaryBankTranferName = "";
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("BulkCTCInsert");
                worksheet.Range(1, 1, 1, 35).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_CmpID", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                var GetBulkInsert_CTC = DapperORM.ExecuteSP<dynamic>("sp_Template_BulkInsert_CTC", paramList).ToList();

                if (GetBulkInsert_CTC.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetBulkInsert_CTC);

                worksheet.Cell(2, 1).InsertTable(dt, false);

                // Set the header row name
                worksheet.Cell(1, 1).Value = "Bulk CTC Insert";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // Adjust all columns
                                                        // worksheet.Row(3).Delete();
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

        #region Module_Payroll_BulkCTCInsert
        [HttpPost]
        public ActionResult Module_Payroll_BulkCTCInsert(HttpPostedFileBase AttachFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 543;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                ViewBag.CompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();

                var results = DapperORM.DynamicQueryMultiple(@"Select CategoryId as Id,CategoryName as Name from Payroll_CTC_Category where Deactivate=0");
                ViewBag.CTC_Category = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                ViewBag.BranchName = "";
                ViewBag.BudgetName = "";
                ViewBag.SalaryBankTranferName = "";

                if (AttachFile.ContentLength > 0)
                {
                    using (var attachStream = AttachFile.InputStream)
                    {
                        attachStream.Position = 0;
                        XLWorkbook xlWorkbook = new XLWorkbook(attachStream);
                        var worksheet = xlWorkbook.Worksheets.Worksheet(1);

                        int headerRow = 2;       // Header row
                        int dataStartRow = 3;    // Data starts from row 3

                        int lastColumn = worksheet.Row(headerRow).LastCellUsed()?.Address.ColumnNumber ?? 43;

                        // Extract headers
                        List<string> headers = new List<string>();
                        for (int col = 1; col <= lastColumn; col++)
                        {
                            headers.Add(worksheet.Cell(headerRow, col).GetFormattedString());
                        }

                        List<Dictionary<string, string>> excelDataList = new List<Dictionary<string, string>>();
                        HashSet<string> employeeNoSet = new HashSet<string>();
                        List<string> duplicateRows = new List<string>();

                        int row = dataStartRow;
                        while (!worksheet.Cell(row, 1).IsEmpty())
                        {
                            var rowData = new Dictionary<string, string>();

                            for (int col = 1; col <= lastColumn; col++)
                            {
                                var cell = worksheet.Cell(row, col);
                                string columnValue;

                                if (cell.DataType == XLDataType.DateTime)
                                {
                                    columnValue = cell.GetDateTime().ToString("yyyy-MM-dd");
                                }
                                else
                                {
                                    columnValue = cell.GetFormattedString();
                                }

                                string header = headers[col - 1];
                                rowData[header] = columnValue;
                            }

                            // Extract EmployeeNo (change the key if your column name is different)
                            string employeeNo = rowData.ContainsKey("Employee No") ? rowData["Employee No"] : "";

                            if (!string.IsNullOrWhiteSpace(employeeNo) && !employeeNoSet.Add(employeeNo))
                            {
                                duplicateRows.Add($"Row {row}: Duplicate Employee No ({employeeNo})");
                            }

                            excelDataList.Add(rowData);
                            row++;
                        }

                        // If any duplicates are found, stop processing and notify the user
                        if (duplicateRows.Any())
                        {
                            TempData["Message"] = $"Duplicate entries found:\n{string.Join("\n", duplicateRows)}";
                            TempData["Title"] = "Duplicate Entries Detected";
                            TempData["Icon"] = "error";
                            return RedirectToAction("Module_Payroll_BulkCTCInsert", "Module_Payroll_BulkCTCInsert");
                        }

                        // Store in ViewBag
                        ViewBag.ExcelData = excelDataList;
                        ViewBag.LastColumn = lastColumn;
                        ViewBag.Headers = headers;
                    }
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

        #region SaveUpdate
        int EmployeeNo_Check = 0;
        int CompanyID_Id = 0;
        int Branch_Id = 0;
        int Category_Id = 0;
        int SalaryTranferName_Id = 0;
        int BudgetName_Id = 0;

        int List_CompanyID = 0;
        int List_BUId = 0;
        int List_Category_Id = 0;
        int List_SalaryTranferName_Id = 0;
        int List_BudgetName_Id = 0;
        [HttpPost]
        public ActionResult SaveUpdate(List<Dictionary<string, string>> ObjBulkCTCInsert, int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 543;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var a = ObjBulkCTCInsert[0].Values;
                dt_companyName = objcon.GetDataTable("select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0");
                dt_company = objcon.GetDataTable("select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0  and  CompanyId in ( Select distinct CmpID from UserBranchMapping where IsActive=1 and EmployeeID=" + Session["EmployeeId"] + ") order by CompanyName ;");
                dt_BU = objcon.GetDataTable("Select UserBranchMapping.BranchId , Mas_Branch.BranchName,Convert (int,UserBranchMapping.CmpID) as CmpId from UserBranchMapping,Mas_Branch where UserBranchMapping.IsActive=1 and Mas_Branch.deactivate=0 and UserBranchMapping.BranchID=Mas_Branch.branchid and UserBranchMapping.EmployeeID='" + Session["EmployeeId"] + "'");
                dt_CategoryName = objcon.GetDataTable("Select  Convert (int,Payroll_CTC_Category.CategoryId) as CategoryId,Payroll_CTC_Category.CategoryName as Name from Payroll_CTC_Category where Deactivate=0");
                dt_SalaryBankTranfer = objcon.GetDataTable("Select Convert (int,Payroll_CompanyBank.CompanyBankId) as CompanyBankId ,BankName as Name, Convert (int,Payroll_CompanyBank.CompanyBankBUId) as CompanyBankBUId ,Convert (int,Payroll_CompanyBank.CompanyBankCmpID) as CompanyBankCmpID from Payroll_CompanyBank where Payroll_CompanyBank.Deactivate=0  and CompanyBankBUId in ( Select distinct BranchID from UserBranchMapping where IsActive=1 and EmployeeID=" + Session["EmployeeId"] + " and CmpId=" + CmpId + ")");
                dt_BudgetName = objcon.GetDataTable("Select   Convert (int,Payroll_Budget.BudgetId) as BudgetId,BudgetName as Name,Convert (int,Payroll_BudgetMapping.BudgetMappingBuId) as BudgetMappingBuId ,Convert (int,Payroll_BudgetMapping.BudgetMappingCmpId) as BudgetMappingCmpId from Payroll_Budget,Payroll_BudgetMapping where Payroll_BudgetMapping.BudgetMappingBugetId=Payroll_Budget.BudgetId and Payroll_Budget.Deactivate=0 and Payroll_BudgetMapping.Deactivate=0 AND Payroll_BudgetMapping.BudgetMappingBuId in ( Select distinct BranchID from UserBranchMapping where IsActive=1 and EmployeeID=" + Session["EmployeeId"] + " and CmpId=" + CmpId + " )");
                dt_CheckEmployeeNo = objcon.GetDataTable("Select EmployeeNo,  Convert (int,Mas_Employee.EmployeeId) as  EmployeeId from Mas_Employee where CmpID=" + CmpId + " and Deactivate=0 and EmployeeLeft=0");


                DataTable tbl_BulkInsertCTC = new DataTable();
                tbl_BulkInsertCTC.Columns.Add("EmployeeNo", typeof(string));
                tbl_BulkInsertCTC.Columns.Add("MasterHead", typeof(string));
                tbl_BulkInsertCTC.Columns.Add("HeadName", typeof(string));
                tbl_BulkInsertCTC.Columns.Add("HeadValues", typeof(string));

                // Safety check
                if (ObjBulkCTCInsert != null && ObjBulkCTCInsert.Any())
                {
                    EmployeeNo_Check = 0;
                    CompanyID_Id = 0;
                    Branch_Id = 0;

                    List_CompanyID = 0;
                    List_BUId = 0;
                    // Get headers (assumes all dictionaries have same keys)
                    var headers = ObjBulkCTCInsert.First().Keys.ToList();

                    // Index 0-based: index 2 means 3rd column (assuming this is "EmployeeNo")
                    string empNoHeader = headers.Count > 2 ? headers[2] : null;
                    string CompanyNameHeader = headers.Count > 0 ? headers[0] : null;
                    string BranchNameHeader = headers.Count > 1 ? headers[1] : null;
                    string CategoryNameHeader = headers.Count > 5 ? headers[5] : null;
                    string SalaryTranferNameHeader = headers.Count > 6 ? headers[6] : null;
                    string BudgetNameHeader = headers.Count > 7 ? headers[7] : null;
                    string SalaryModeHeader = headers.Count > 8 ? headers[8] : null;

                    int rowno = 1; // Start row counter

                    foreach (var row in ObjBulkCTCInsert)
                    {
                        // Extract EmployeeNo from the 3rd column
                        string employeeNo = empNoHeader != null && row.ContainsKey(empNoHeader) ? row[empNoHeader] : "";
                        string CompanyName = CompanyNameHeader != null && row.ContainsKey(CompanyNameHeader) ? row[CompanyNameHeader] : "";
                        string BranchName = BranchNameHeader != null && row.ContainsKey(BranchNameHeader) ? row[BranchNameHeader] : "";
                        string CategoryName = CategoryNameHeader != null && row.ContainsKey(CategoryNameHeader) ? row[CategoryNameHeader] : "";
                        string SalaryTranferName = SalaryTranferNameHeader != null && row.ContainsKey(SalaryTranferNameHeader) ? row[SalaryTranferNameHeader] : "";
                        string BudgetName = BudgetNameHeader != null && row.ContainsKey(BudgetNameHeader) ? row[BudgetNameHeader] : "";
                        string SalaryModeName = SalaryModeHeader != null && row.ContainsKey(SalaryModeHeader) ? row[SalaryModeHeader] : "";

                        if (string.IsNullOrWhiteSpace(employeeNo))
                        {
                            var Message = "Enter Employee no in SrNo " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeNoCheck(employeeNo.ToString(), out EmployeeNo_Check);
                        if (EmployeeNo_Check == 0)
                        {
                            var Message = "The selected company does not have an assigned employee number " + employeeNo;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (string.IsNullOrWhiteSpace(CompanyName))
                        {
                            var Message = "Enter Company Name in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetCompany_Id_Name(CompanyName, out CompanyID_Id);
                        List_CompanyID = CompanyID_Id;
                        if (CompanyID_Id == 0)
                        {
                            var Message = "Company Name not valid in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetCompany_Id(CompanyName, out CompanyID_Id);
                        List_CompanyID = CompanyID_Id;
                        if (List_CompanyID == 0)
                        {
                            var Message = "Company name is not mapped to you in Row no " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (string.IsNullOrWhiteSpace(BranchName))
                        {
                            var Message = "Enter Business unit in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetBranch_Id_Name(BranchName, out Branch_Id);
                        if (Branch_Id == 0)
                        {
                            var Message = "Business unit not valid in Row No " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetBranch_Id(BranchName, List_CompanyID, out Branch_Id);
                        List_BUId = Branch_Id;
                        if (List_BUId == 0)
                        {
                            var Message = "Business unit is not mapped to you in Row No " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (string.IsNullOrWhiteSpace(CategoryName))
                        {
                            var Message = "Enter Category name in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetCategory_Id(CategoryName, out Category_Id);
                        if (Category_Id == 0)
                        {
                            var Message = "Category name not valid in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (string.IsNullOrWhiteSpace(SalaryTranferName))
                        {
                            var Message = "Enter Salary Transfer Name in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetSalaryTranferName_Id(SalaryTranferName, List_CompanyID, List_BUId, out SalaryTranferName_Id);
                        List_SalaryTranferName_Id = SalaryTranferName_Id;
                        if (SalaryTranferName_Id == 0)
                        {
                            var Message = "Salary transfer name not valid in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (string.IsNullOrWhiteSpace(BudgetName))
                        {
                            var Message = "Enter Budget name in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetBudgetName_Id(BudgetName, List_CompanyID, List_BUId, out BudgetName_Id);
                        List_BudgetName_Id = BudgetName_Id;
                        if (BudgetName_Id == 0)
                        {
                            var Message = "Budget name not valid in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (string.IsNullOrWhiteSpace(SalaryModeName))
                        {
                            var Message = "Enter Salary mode in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        else if (SalaryModeName != "Bank" && SalaryModeName != "Cash" && SalaryModeName != "Cheque")
                        {
                            var Message = "Salary mode must be either Bank, Cash, or Cheque in Row No  " + rowno;
                            var Icon = "error";
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        rowno++;

                        foreach (var kvp in row)
                        {
                            if (kvp.Key != empNoHeader) // Skip the column used for EmployeeNo
                            {
                                DataRow dr = tbl_BulkInsertCTC.NewRow();
                                dr["EmployeeNo"] = employeeNo;
                                dr["MasterHead"] = ""; // Update if needed
                                dr["HeadName"] = kvp.Key;
                                dr["HeadValues"] = kvp.Value;
                                tbl_BulkInsertCTC.Rows.Add(dr);
                            }
                        }
                    }
                }
                DynamicParameters ARparam = new DynamicParameters();
                ARparam.Add("@tbl_BulkInsertCTC", tbl_BulkInsertCTC.AsTableValuedParameter("tbl_BulkInsertCTC"));
                ARparam.Add("@p_CmpID", CmpId);
                ARparam.Add("@p_EmployeeId", Session["EmployeeId"]);
                ARparam.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                ARparam.Add("@p_MachineName", Dns.GetHostName().ToString());
                ARparam.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                ARparam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter  
                //ARparam.Add("@tbl_dt", dt, DbType.Object, ParameterDirection.Input);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_BulkInsert_CTC", ARparam).ToList();
                TempData["Message"] = ARparam.Get<string>("@p_msg");
                TempData["Icon"] = ARparam.Get<string>("@p_Icon");
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], GetData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ConvertToDataTable
        public static DataTable ConvertToDataTable<T>(IEnumerable<T> data)
        {
            DataTable table = new DataTable();
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                table.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo property in properties)
                {
                    row[property.Name] = property.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
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

        #region GetBudgetName
        [HttpGet]
        public ActionResult GetBudgetName(int? CmpId, int? BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select CompanyBankId as Id,BankName as Name from Payroll_CompanyBank where Deactivate=0 and CompanyBankCmpID=" + CmpId + " and CompanyBankBUId=" + BranchId + "  Order By IsDefault Desc");
                var GetCompanyBank = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "Select BudgetId as Id,BudgetName as Name from Payroll_Budget,Payroll_BudgetMapping where Payroll_BudgetMapping.BudgetMappingBugetId=Payroll_Budget.BudgetId and Payroll_Budget.Deactivate=0 and Payroll_BudgetMapping.Deactivate=0 AND Payroll_BudgetMapping.BudgetMappingBuId =" + BranchId + "");
                var GetBudgetMapping = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();

                return Json(new { GetCompanyBank = GetCompanyBank, GetBudgetMapping = GetBudgetMapping }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployeeNoCheck
        void GetEmployeeNoCheck(string EmployeeNo, out int EmployeeNo_Check)
        {
            try
            {
                var matchingRows = from row in dt_CheckEmployeeNo.AsEnumerable()
                                   where string.Equals(row.Field<string>("EmployeeNo"), EmployeeNo, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varEmployeeNo_Id = matchingRows.LastOrDefault();

                //var varCompany_Id = (from row in dt_company.AsEnumerable()
                //                     where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                     select row).Last();
                if (varEmployeeNo_Id == null)
                {
                    EmployeeNo_Check = 0;
                }
                else
                {
                    EmployeeNo_Check = Convert.ToInt32(varEmployeeNo_Id["EmployeeId"]);
                }
            }
            catch
            {
                EmployeeNo_Check = 0;
            }
        }


        void GetCompany_Id_Name(string Name, out int Company_Id)
        {
            try
            {
                var matchingRows = from row in dt_companyName.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varCompany_Id = matchingRows.LastOrDefault();

                //var varCompany_Id = (from row in dt_company.AsEnumerable()
                //                     where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                     select row).Last();
                if (varCompany_Id == null)
                {
                    Company_Id = 0;
                }
                else
                {
                    Company_Id = Convert.ToInt32(varCompany_Id["Id"]);
                }
            }
            catch
            {


                Company_Id = 0;
            }
        }

        void GetCompany_Id(string Name, out int Company_Id)
        {
            try
            {
                var matchingRows = from row in dt_company.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varCompany_Id = matchingRows.LastOrDefault();

                //var varCompany_Id = (from row in dt_company.AsEnumerable()
                //                     where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                     select row).Last();
                if (varCompany_Id == null)
                {
                    Company_Id = 0;
                }
                else
                {
                    Company_Id = Convert.ToInt32(varCompany_Id["Id"]);
                }
            }
            catch
            {


                Company_Id = 0;
            }
        }

        void GetBranch_Id_Name(string Name, out int Branch_Id)
        {

            try
            {

                //var matchingRows = from row in dt_Contractor.AsEnumerable()
                //                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                   select row;

                var matchingRows = from row in dt_BU.AsEnumerable()
                                   where string.Equals(row.Field<string>("BranchName"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;
                var varBranch_Id = matchingRows.LastOrDefault();
                if (varBranch_Id == null)
                {
                    Branch_Id = 0;
                }
                else
                {
                    Branch_Id = Convert.ToInt32(varBranch_Id["BranchId"]);
                }
            }
            catch (Exception ex)
            {
                Branch_Id = 0;
            }
        }

        void GetBranch_Id(string Name, int companyId, out int Branch_Id)
        {

            try
            {

                var matchingRows = from row in dt_BU.AsEnumerable()
                                   where string.Equals(row.Field<string>("BranchName"), Name, StringComparison.OrdinalIgnoreCase)
                                         && row.Field<int>("CmpId") == Convert.ToInt16(companyId)
                                   select row;
                var varBranch_Id = matchingRows.LastOrDefault();
                if (varBranch_Id == null)
                {
                    Branch_Id = 0;
                }
                else
                {
                    Branch_Id = Convert.ToInt32(varBranch_Id["BranchId"]);
                }
            }
            catch (Exception ex)
            {
                Branch_Id = 0;
            }
        }

        void GetCategory_Id(string Name, out int Category_Id)
        {

            try
            {
                var matchingRows = from row in dt_CategoryName.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varCategory_Id = matchingRows.LastOrDefault();
                if (varCategory_Id == null)
                {
                    Category_Id = 0;
                }
                else
                {
                    Category_Id = Convert.ToInt32(varCategory_Id["CategoryId"]);
                }
            }
            catch (Exception ex)
            {
                Category_Id = 0;
            }
        }

        void GetSalaryTranferName_Id(string Name, int companyId, int BranchId, out int SalaryTranferName_Id)
        {

            try
            {


                var matchingRows = from row in dt_SalaryBankTranfer.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                         && row.Field<int>("CompanyBankCmpID") == Convert.ToInt16(companyId)
                                         && row.Field<int>("CompanyBankBUId") == Convert.ToInt16(BranchId)
                                   select row;
                var varSalaryTranferName_Id = matchingRows.LastOrDefault();
                if (varSalaryTranferName_Id == null)
                {
                    SalaryTranferName_Id = 0;
                }
                else
                {
                    SalaryTranferName_Id = Convert.ToInt32(varSalaryTranferName_Id["CompanyBankId"]);
                }
            }
            catch (Exception ex)
            {
                SalaryTranferName_Id = 0;
            }
        }

        void GetBudgetName_Id(string Name, int companyId, int BranchId, out int BudgetName_Id)
        {

            try
            {

                var matchingRows = from row in dt_BudgetName.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                         && row.Field<int>("BudgetMappingCmpId") == Convert.ToInt16(companyId)
                                         && row.Field<int>("BudgetMappingBuId") == Convert.ToInt16(BranchId)
                                   select row;
                var varBudgetName_Id = matchingRows.LastOrDefault();
                if (varBudgetName_Id == null)
                {
                    BudgetName_Id = 0;
                }
                else
                {
                    BudgetName_Id = Convert.ToInt32(varBudgetName_Id["BudgetId"]);
                }
            }
            catch (Exception ex)
            {
                BudgetName_Id = 0;
            }
        }

        #endregion

    }
}