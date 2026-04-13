using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Report_Recruitment
{
    public class Report_Recruitment_AgencyWiseController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Reports/Report_Recruitment_AgencyWise

        #region Report_Recruitment_AgencyWise
        public ActionResult Report_Recruitment_AgencyWise()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 613;
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
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and( month(mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (month(LeavingDate)='" + DateTime.Now.Date.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ")");
                var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();
                ViewBag.GetContractor = GetContractorDropdown;


                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                                                 Select AgencyId as Id,AgencyName as Name from Recruitment_Agency where Deactivate=0 ");
                ViewBag.DepartmentName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.AgencyName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                //ViewBag.DepartmentName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.DesignationName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.AgencyName = results.Read<AllDropDownClass>().ToList();
                ViewBag.DailyAttendanceReport = "";
                ViewBag.SubDepartmentName = "";
                ViewBag.LineName = "";
                ViewBag.SubUnit = "";
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

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromDate, int? DepartmentId, int? DesignationId, int? AgencyId, DateTime ToDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("AgencyWiseDetails");
                worksheet.Range(1, 1, 1, 35).Merge();
                worksheet.SheetView.FreezeRows(2); 
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                var Query = "";
                if (CmpId != null)
                {
                    Query = " and Mas_CompanyProfile.CompanyId=" + CmpId;
                    if (BranchId != null)
                    {
                        Query = Query + " and Mas_Branch.BranchId=" + BranchId + "";
                    }
                    else
                    {
                        Query = Query + " and Mas_Branch.BranchId in (select BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and UserBranchMapping.CmpID = " + CmpId + " and UserBranchMapping.IsActive = 1)";
                    }

                    if (DepartmentId != null)
                    {
                        Query = Query + " and Mas_Department.DepartmentId=" + DepartmentId + "";
                    }

                    if (AgencyId != null)
                    {
                        Query = Query + " and Recruitment_Agency.AgencyId=" + AgencyId + "";
                    }

                    if (DesignationId != null)
                    {
                        Query = Query + " and Mas_Designation.DesignationId=" + DesignationId + "";
                    }

                    var GetFromDate = FromDate.ToString("yyyy-MM-dd");
                    var GetToDate = ToDate.ToString("yyyy-MM-dd");
                    if (FromDate != null)
                    {
                        Query = Query + " and CONVERT(date, Recruitment_Resume.DocDate) BETWEEN  '" + GetFromDate + "' AND '" + GetToDate + "'";
                    }
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@P_Qry", Query);
                var Recruitment_AgencyDetails = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Recruitment_AgencyDetails", paramList).ToList();
                if (Recruitment_AgencyDetails.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(Recruitment_AgencyDetails);
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
                worksheet.Cell(1, 1).Value = "Agency Wise Details - (" + FromDate.ToString("dd/MMM/yyyy") + " - " + ToDate.ToString("dd/MMM/yyyy") + ")";
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