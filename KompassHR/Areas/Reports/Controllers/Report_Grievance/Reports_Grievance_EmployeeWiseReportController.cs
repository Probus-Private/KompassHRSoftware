using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Report_Grievance
{
    public class Reports_Grievance_EmployeeWiseReportController : Controller
    {
        #region Main View
        // GET: Reports/Reports_Grievance_EmployeeWiseReport
        public ActionResult Reports_Grievance_EmployeeWiseReport(DailyAttendanceReportFilter MasReport)
        {
            try
             {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 758;
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

                DynamicParameters paramEmpName = new DynamicParameters();
                //paramEmpName.Add("@query", "SELECT DISTINCT Mas_Employee.EmployeeId AS[Id], Mas_Employee.EmployeeName AS [Name] FROM Mas_Employee INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId  WHERE Mas_Employee.Deactivate = 0 AND Mas_CompanyProfile.CompanyId = '" + CmpId + "'");
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and( month(mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (month(LeavingDate)='" + DateTime.Now.Date.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();


                DynamicParameters paramCatName = new DynamicParameters();
                paramCatName.Add("@query", "Select Distinct GrievanceCategoryId As[Id],GrievanceCategory As[Name] from Employee_Grievance_Category;");
                ViewBag.CategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCatName).ToList();


                DynamicParameters paramSubCatName = new DynamicParameters();
                paramSubCatName.Add("@query", "Select Distinct GrievanceSubCategoryId As[Id],GrievanceSubCategory As[Name] from Employee_Grievance_SubCategory;");
                ViewBag.SubCategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSubCatName).ToList();

                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName; ");
                ViewBag.DepartmentName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
         
                return View(MasReport);
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
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        #region GetDepartmentName
        [HttpGet]
        public ActionResult GetDepartmentName(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT DISTINCT Mas_Department.DepartmentId AS[Id], Mas_Department.DepartmentName AS [Name] FROM Mas_Employee INNER JOIN Mas_Department ON Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Mas_CompanyProfile.CompanyId = '" + CmpId+"'AND Mas_Employee.EmployeeBranchId = '"+ BranchId + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                // return Json(new { data = data }, JsonRequestBehavior.AllowGet);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        #region GetDesignationName
        [HttpGet]
        public ActionResult GetDesignationName(int DepartmentId, int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT DISTINCT Mas_Designation.DesignationId AS[Id], Mas_Designation.DesignationName AS [Name] FROM Mas_Employee INNER JOIN Mas_Department ON Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId INNER JOIN Mas_Designation ON Mas_Employee.EmployeeDesignationID = Mas_Designation.DesignationId INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Mas_Department.DepartmentId = '" + DepartmentId + "' AND Mas_CompanyProfile.CompanyId = '" + CmpId + "'AND Mas_Employee.EmployeeBranchId = '" + BranchId + "'");

               // param.Add("@query", "SELECT DISTINCT Mas_Department.DepartmentId AS[Id], Mas_Department.DepartmentName AS [Name] FROM Mas_Employee INNER JOIN Mas_Department ON Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Mas_CompanyProfile.CompanyId = '" + CmpId + "'AND Mas_Employee.EmployeeBranchId = '" + BranchId + "'");
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

        #region GetSubCategoryName
        [HttpGet]
        public ActionResult GetSubCategoryName(  int GrievanceCategoryId  )
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT DISTINCT Employee_RaiseGrievance.GrievanceSubCategoryId AS[Id], Employee_Grievance_SubCategory.GrievanceSubCategory AS [Name] FROM Employee_RaiseGrievance INNER JOIN Employee_Grievance_SubCategory ON Employee_RaiseGrievance.GrievanceSubCategoryId = Employee_Grievance_SubCategory.GrievanceSubCategoryId INNER JOIN Mas_Employee ON Employee_RaiseGrievance.EmployeeId = Mas_Employee.EmployeeId INNER JOIN Mas_Department ON Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId INNER JOIN Mas_Designation ON Mas_Employee.EmployeeDesignationID = Mas_Designation.DesignationId INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Employee_RaiseGrievance.GrievanceCategoryId = '" + GrievanceCategoryId + "'");
              //  param.Add("@query", "SELECT DISTINCT Employee_RaiseGrievance.GrievanceSubCategoryId AS[Id], Employee_Grievance_SubCategory.GrievanceSubCategory AS [Name] FROM Employee_RaiseGrievance INNER JOIN Employee_Grievance_SubCategory ON Employee_RaiseGrievance.GrievanceSubCategoryId = Employee_Grievance_SubCategory.GrievanceSubCategoryId INNER JOIN Mas_Employee ON Employee_RaiseGrievance.EmployeeId = Mas_Employee.EmployeeId INNER JOIN Mas_Department ON Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId INNER JOIN Mas_Designation ON Mas_Employee.EmployeeDesignationID = Mas_Designation.DesignationId INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Employee_RaiseGrievance.GrievanceCategoryId = '" + GrievanceCategoryId + "' AND Mas_CompanyProfile.CompanyId = '" + CmpId + "'AND Mas_Employee.EmployeeBranchId = '" + BranchId + "'");

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

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT DISTINCT Mas_Employee.EmployeeId AS[Id], Mas_Employee.EmployeeName AS [Name] FROM Mas_Employee INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId  WHERE Mas_Employee.Deactivate = 0 AND Mas_CompanyProfile.CompanyId = '" + CmpId + "'AND Mas_Employee.EmployeeBranchId = '" + BranchId + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                // return Json(new { data = data }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromDate, DateTime ToDate, int? EmployeeId,int?DepartmentId,int? DesignationId, String Status,String Gender,int? GrievanceCategoryId,int? GrievanceSubCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("EmployeeWiseReport");
                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2);
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();


                DynamicParameters paramList = new DynamicParameters();
               // paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_FromDate", FromDate);
                paramList.Add("@p_ToDate", ToDate);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_EmployeeId", EmployeeId);
                paramList.Add("@p_DepartmentId", DepartmentId);
                paramList.Add("@p_DesignationId", DesignationId);
                paramList.Add("@p_Status", Status);
                paramList.Add("@p_Gender", Gender);
                paramList.Add("@p_GrievanceCategoryId", GrievanceCategoryId);
                paramList.Add("@p_GrievanceSubCategoryId", GrievanceSubCategoryId);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Grievance_EmployeeWiseGrievance", paramList).ToList();
                if (GetData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetData);
                worksheet.Cell(2, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();

                var lastRow = worksheet.Row(totalRows + 1);
                lastRow.Style.Font.Bold = false;

                lastRow.Style.Font.FontColor = XLColor.Black;
                lastRow.Style.Font.FontSize = 10;
                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(1, 1).Value = "Employee Wise Report - (" + FromDate.ToString("dd/MMM/yyyy") + " to " + ToDate.ToString("dd/MMM/yyyy") + ")";
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