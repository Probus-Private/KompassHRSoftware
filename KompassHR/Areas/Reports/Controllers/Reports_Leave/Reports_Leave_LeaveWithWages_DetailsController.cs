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
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Leave
{
    public class Reports_Leave_LeaveWithWages_DetailsController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Reports_Leave_LeaveWithWages_Details
        #region Reports_Leave_LeaveWithWages_Details
        public ActionResult Reports_Leave_LeaveWithWages_Details(LeaveWithWage Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 410;
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
                DynamicParameters param6 = new DynamicParameters();
                param6.Add("@p_employeeid", Session["EmployeeId"]);
                param6.Add("@p_CmpId", CmpId);
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param6).ToList();
                ViewBag.BranchName = BranchName;
                var BUID = BranchName[0].Id;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + BUID + " and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid =" + BUID + "  and (year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (year(mas_employee.LeavingDate)='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                //if (Obj.EmployeeId != 0)
                //{
                //    DynamicParameters param2 = new DynamicParameters();
                //    param2.Add("@p_employeeid", Session["EmployeeId"]);
                //    param2.Add("@p_CmpId", Obj.CmpId);
                //    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param2).ToList();
                //    //var GetMonth = "";
                //    //var GetMonthYear = "";
                //    //if (Obj.Month.HasValue)
                //    //{
                //    //    GetMonth = Obj.Month.Value.ToString("MM");
                //    //    GetMonthYear = Obj.Month.Value.ToString("yyyy");
                //    //}

                //    DynamicParameters param3 = new DynamicParameters();
                //    param3.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + Obj.BranchId + " and month( Atten_InOut.InOutDate )='" + Obj.Year + "' and year( Atten_InOut.InOutDate )='" + Obj.Year + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid =" + Obj.BranchId + " and( year (mas_employee.JoiningDate)<='" + Obj.Year + "') and (year(mas_employee.LeavingDate)='" + Obj.Year + "' or mas_employee.LeavingDate is null) order by Name");
                //    ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();

                //    //DynamicParameters param3 = new DynamicParameters();
                //    //param3.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and month( InOutMonthYear)=" + GetMonth + " and Year(InOutMonthYear)=" + GetMonthYear + " and InOutBranchId=" + Obj.BranchId + " ) order by Name");
                //    //var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                //    //ViewBag.EmployeeName = EmployeeName;

                //    DynamicParameters param = new DynamicParameters();
                //    param.Add("@p_Year", Obj.Year);
                //    param.Add("@p_EmployeeId", Obj.EmployeeId);
                //    var LeaveWithWagesRegister = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Leave_LeaveWithWagesRegister_Detail", param).ToList();
                //    ViewBag.GetLeaveWithWagesRegister = LeaveWithWagesRegister;

                //    DynamicParameters EmployeeDetails = new DynamicParameters();
                //    EmployeeDetails.Add("@query", @"Select Mas_Employee.EmployeeName, DepartmentName, DesignationName, EmployeeNo,Mas_Branch.BranchName,Mas_CompanyProfile.CompanyName,Contractor_Master.ContractorName,REPLACE(convert(nvarchar(12),Mas_Employee.JoiningDate,106),' ','/') as JoiningDate, REPLACE(convert(nvarchar(12),Mas_Employee.LeavingDate,106),' ','/') as LeavingDate  ,Mas_Employee_Address.PermanentPostelAddress as Address from Mas_Employee
                //                                    join Mas_Department on Mas_Department.DepartmentId = Mas_Employee.EmployeeDepartmentID
                //                                    join Mas_Designation on Mas_Designation.DesignationId = Mas_Employee.EmployeeDesignationID
                //                                    join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID
                //                                    join Mas_Branch on Mas_Branch.BranchId = Mas_Employee.EmployeeBranchId
                //                                    join Contractor_Master on Contractor_Master.ContractorId = Mas_Employee.ContractorID
                //                                    join Mas_Employee_Address on Mas_Employee_Address.AddressEmployeeId = Mas_Employee.EmployeeId
                //                                    Where Mas_Employee.EmployeeId =" + Obj.EmployeeId + " and Mas_Employee.Deactivate = 0 and Mas_Employee_Address.Deactivate=0");
                //    var datas = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", EmployeeDetails).ToList();
                //    ViewBag.GetEmployeeDetails = datas;



                //}
                //else
                //{
                //    ViewBag.GetLeaveWithWagesRegister = "";
                //    ViewBag.GetEmployeeDetails = "";
                //}
                return View(Obj);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Monthly Attendance DropDown
        [HttpGet]
        public ActionResult GetMonthlyBusinessUnit(int CmpId, int GetMonthYear)
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
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                var GetBranchId = Branch[0].Id;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and year( Atten_InOut.InOutDate )='" + GetMonthYear + "' and EmployeeId<>1 and InOutBranchId=" + GetBranchId + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + GetBranchId + " and (year (mas_employee.JoiningDate)<='" + GetMonthYear + "')  and (year(mas_employee.LeavingDate)='" + GetMonthYear + "' or mas_employee.LeavingDate is null) order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                return Json(new { EmployeeName = EmployeeName, Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        [HttpGet]
        public ActionResult GetMonthlyEmployeeName(int CmpId, int BranchId, int GetMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0  and year( Atten_InOut.InOutDate )='" + GetMonth + "' and EmployeeId<>1 and InOutBranchId=" + BranchId + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + BranchId + "  and (year (mas_employee.JoiningDate)<='" + GetMonth + "')  and ( year(mas_employee.LeavingDate)='" + GetMonth + "' or mas_employee.LeavingDate is null) order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult GetMonthlyEmployeeNameDateWise(int BranchId, int GetMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0  and year( Atten_InOut.InOutDate )='" + GetMonth + "' and EmployeeId<>1 and InOutBranchId=" + BranchId + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + BranchId + " and (year (mas_employee.JoiningDate)<='" + GetMonth + "')  and (year(mas_employee.LeavingDate)='" + GetMonth + "' or mas_employee.LeavingDate is null) order by Name");
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
        public ActionResult DownloadExcelFile(int? Year, int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("ContractorVSLine");
                worksheet.Range(1, 1, 1, 3).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Year", Year);
                param.Add("@p_EmployeeId",EmployeeId);
                var LeaveWithWagesRegister = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Leave_LeaveWithWagesRegister_Detail", param).ToList();

                //DynamicParameters EmployeeDetails = new DynamicParameters();
                //EmployeeDetails.Add("@query", @"Select Mas_Employee.EmployeeName, DepartmentName, DesignationName, EmployeeNo,Mas_Branch.BranchName,Mas_CompanyProfile.CompanyName,Contractor_Master.ContractorName,REPLACE(convert(nvarchar(12),Mas_Employee.JoiningDate,106),' ','/') as JoiningDate, REPLACE(convert(nvarchar(12),Mas_Employee.LeavingDate,106),' ','/') as LeavingDate  ,Mas_Employee_Address.PermanentPostelAddress as Address from Mas_Employee
                //                                    join Mas_Department on Mas_Department.DepartmentId = Mas_Employee.EmployeeDepartmentID
                //                                    join Mas_Designation on Mas_Designation.DesignationId = Mas_Employee.EmployeeDesignationID
                //                                    join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID
                //                                    join Mas_Branch on Mas_Branch.BranchId = Mas_Employee.EmployeeBranchId
                //                                    join Contractor_Master on Contractor_Master.ContractorId = Mas_Employee.ContractorID
                //                                    join Mas_Employee_Address on Mas_Employee_Address.AddressEmployeeId = Mas_Employee.EmployeeId
                //                                    Where Mas_Employee.EmployeeId =" + Obj.EmployeeId + " and Mas_Employee.Deactivate = 0 and Mas_Employee_Address.Deactivate=0");
                //var datas = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", EmployeeDetails).ToList();

                if (LeaveWithWagesRegister.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(LeaveWithWagesRegister);
                worksheet.Cell(2, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();

                // Convert the column values to numbers and calculate the sum
                for (int col = dt.Columns.Count; col >= 3; col--) // Loop backwards to avoid index issues when deleting columns
                {
                    double sum = 0;
                    for (int row = 3; row <= totalRows + 1; row++)
                    {
                        var cell = worksheet.Cell(row, col);
                        double value;
                        if (double.TryParse(cell.GetValue<string>(), out value))
                        {
                            cell.Value = value;
                            sum += value;
                        }
                    }

                    // If the sum is 0, delete the entire column
                    if (sum == 0)
                    {
                        worksheet.Column(col).Delete();
                    }
                    else
                    {
                        // Insert the total value at the end of the column if the sum is non-zero
                        var totalCell = worksheet.Cell(totalRows + 2, col);
                        totalCell.Value = sum;
                        totalCell.Style.Font.Bold = true;
                        totalCell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        totalCell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        totalCell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        totalCell.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    }
                }
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
               // worksheet.Cell(1, 1).Value = "Contractor VS Line Report - (" + txtDate.ToString("dd/MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all columns

                var usedColumnsCount = worksheet.ColumnsUsed().Count();

                // Apply header styling only to the present columns
                var headerRange = worksheet.Range(2, 1, 2, usedColumnsCount); // Apply styles to only the columns that are still in use
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;


                // Merge the first two columns in the total row and add "Total" label
                var totalLabelRange = worksheet.Range(totalRows + 2, 1, totalRows + 2, 2);
                totalLabelRange.Merge();
                totalLabelRange.Value = "Total";
                totalLabelRange.Style.Font.Bold = true;
                totalLabelRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                totalLabelRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                totalLabelRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                totalLabelRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                totalLabelRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

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