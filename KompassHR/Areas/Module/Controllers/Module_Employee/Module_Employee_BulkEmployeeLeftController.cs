using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
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

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_BulkEmployeeLeftController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Employee_BulkEmployeeLeft
        #region BulkEmployeeLeft
        public ActionResult Module_Employee_BulkEmployeeLeft(DailyAttendanceReportFilter OBJ)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 306;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;
                var BUDI = GetComapnyName[0].Id;
                DynamicParameters paramBranch1 = new DynamicParameters();
                paramBranch1.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch1.Add("@p_CmpId", BUDI);
                var BranchNameList = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch1).ToList();
                ViewBag.GetBranchName = BranchNameList;

                DynamicParameters paramGra = new DynamicParameters();
                paramGra.Add("@query", "select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");
                ViewBag.GradeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramGra).ToList();


                DynamicParameters param8 = new DynamicParameters();
                param8.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchNameList[0].Id + " and Contractor_Master.ContractorID<>1");
                var GetContractorDropdown1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param8).ToList();
                ViewBag.ContractorName = GetContractorDropdown1;

                ViewBag.GetExceldata = null;
                //if (OBJ.CmpId != null && OBJ.BranchId == null)
                //{
                //    DynamicParameters paramEmployee = new DynamicParameters();
                //    paramEmployee.Add("@P_Qry", "and EmployeeBranchId in (select BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and UserBranchMapping.CmpID = " + OBJ.CmpId + " and UserBranchMapping.IsActive = 1)  and Mas_Employee.employeeleft=0 order by EmployeeName");
                //    var data = DapperORM.ExecuteSP<dynamic>("sp_GetEmployeeList", paramEmployee).ToList();
                //    ViewBag.BulkEmployeeLeft = data;

                //    DynamicParameters paramBranch = new DynamicParameters();
                //    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                //    paramBranch.Add("@p_CmpId", OBJ.CmpId);
                //    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                //    ViewBag.GetBranchName = BranchName;
                //}
                //else if (OBJ.CmpId != null && OBJ.BranchId != null)
                //{
                //    DynamicParameters paramEmployee = new DynamicParameters();
                //    paramEmployee.Add("@p_Qry", "and EmployeeBranchId=" + OBJ.BranchId + " and Mas_Employee.employeeleft=0   order by EmployeeName");
                //    var data = DapperORM.ExecuteSP<dynamic>("sp_GetEmployeeList", paramEmployee).ToList();
                //    ViewBag.BulkEmployeeLeft = data;

                //    DynamicParameters paramBranch = new DynamicParameters();
                //    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                //    paramBranch.Add("@p_CmpId", OBJ.CmpId);
                //    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                //    ViewBag.GetBranchName = BranchName;
                //}
                //else
                //{
                //    // ViewBag.GetBranchName = "";
                //    ViewBag.BulkEmployeeLeft = "";
                //}

                ViewBag.BulkEmployeeLeft = "";
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult Module_Employee_BulkEmployeeLeft(HttpPostedFileBase AttachFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;
                var BUDI = GetComapnyName[0].Id;
                DynamicParameters paramBranch1 = new DynamicParameters();
                paramBranch1.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch1.Add("@p_CmpId", BUDI);
                var BranchNameList = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch1).ToList();
                ViewBag.GetBranchName = BranchNameList;

                DynamicParameters paramGra = new DynamicParameters();
                paramGra.Add("@query", "select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");
                ViewBag.GradeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramGra).ToList();


                DynamicParameters param8 = new DynamicParameters();
                param8.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchNameList[0].Id + " and Contractor_Master.ContractorID<>1");
                var GetContractorDropdown1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param8).ToList();
                ViewBag.ContractorName = GetContractorDropdown1;

                List<_Employee_BulkEmployeeLeft> excelDataList = new List<_Employee_BulkEmployeeLeft>();

                if (AttachFile.ContentLength > 0)
                {

                    //string directoryPath = Server.MapPath("~/assets/BulkInsert");
                    using (var stream = new MemoryStream())
                    {
                        // Load the uploaded file into the XLWorkbook
                        using (var attachStream = AttachFile.InputStream)
                        {
                            // Reset stream position if necessary
                            attachStream.Position = 0;

                            XLWorkbook xlWorkwook = new XLWorkbook(attachStream);

                            // Do any operations on workbook if necessary
                            xlWorkwook.SaveAs(stream);  // Save the workbook to memory stream

                            int row = 3;
                            if (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() == "")
                            {

                                TempData["Message"] = "Fill in missing information in the first column.";
                                TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                TempData["Icon"] = "error";
                                return RedirectToAction("Module_Employee_BulkEmployeeLeft", "Module_Employee_BulkEmployeeLeft");
                            }
                            while (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() != "")
                            {
                                _Employee_BulkEmployeeLeft BulkInsert = new _Employee_BulkEmployeeLeft();
                                BulkInsert.SrNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString();
                                BulkInsert.EmployeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString();
                                BulkInsert.EmployeeName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                                BulkInsert.ContractorName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 7).GetString();
                                BulkInsert.GradeName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 8).GetString();
                                BulkInsert.JoiningDate = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 9).GetString();
                                var leavingDateCell = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 10);
                                if (leavingDateCell.IsEmpty() || leavingDateCell.GetValue<DateTime?>() == null)
                                {
                                    BulkInsert.LeavingDate = DateTime.Now.ToString("yyyy-MM-dd"); // Assign current date
                                }
                                else
                                {
                                    BulkInsert.LeavingDate = leavingDateCell.GetDateTime().ToString("yyyy-MM-dd");
                                }
                                //BulkInsert.LeavingDate = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 10).GetDateTime().ToString("yyyy-MM-dd");
                                BulkInsert.EmployeeCardNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                                excelDataList.Add(BulkInsert);
                                row++;
                            }
                            ///System.IO.File.Delete(attachStream);
                        }
                    }

                    ViewBag.count = 1;
                    ViewBag.GetExceldata = excelDataList;
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
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters param8 = new DynamicParameters();
                param8.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + Branch[0].Id + " and Contractor_Master.ContractorID<>1");
                var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param8).ToList();
                return Json(new { Branch = Branch, GetContractorDropdown = GetContractorDropdown }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate
        public ActionResult SaveUpdate(List<_Employee_BulkEmployeeLeft> tbldata)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
                //int i = 0;
                if (tbldata != null)
                {

                    foreach (var Data in tbldata)
                    {
                        if (Data.LeavingDate == null)
                        {
                            var Message1 = "Leaving date is manditory in row  " + Data.SrNo;
                            var Icon1 = "error";
                            return Json(new { Message1, Icon1 }, JsonRequestBehavior.AllowGet);
                            //try
                            //{

                            //}
                            //catch (Exception ex)
                            //{
                            //    var Message1 = "Leaving date is manditory" + Data.SrNo;
                            //    var Icon1 = "error";
                            //    return Json(new { Message1, Icon1 }, JsonRequestBehavior.AllowGet);
                            //}

                        }

                        string Employee_Admin = " Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                                " Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeNo ='" + Data.EmployeeNo + "'    " +
                                                " " +
                                                " " +
                                                " Update Mas_Employee set EmployeeLeft=1 ,LeavingDate='" + Convert.ToDateTime(Data.LeavingDate).ToString("yyyy-MM-dd") + "',LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeNo='" + Data.EmployeeNo + "'" +
                                                " " +
                                                " " +
                                                " Update Atten_InOut set Atten_InOut.Deactivate=1,Atten_InOut.InOutRegNote=concat(InOutRegNote,', EmployeeLeft') where Atten_InOut.InOut_EmployeeCardNo='" + Data.EmployeeCardNo + "' and InOutDate >'" + Convert.ToDateTime(Data.LeavingDate).ToString("yyyy-MM-dd") + "' " +
                                                " ";
                        // " Update Atten_InOut set Atten_InOut.Deactivate=1 where Atten_InOut.InOut_EmployeeCardNo='"+ Data.EmployeeCardNo + "' and InOutDate >'"+ Convert.ToDateTime(Data.LeavingDate).ToString("yyyy-MM-dd") + "' " +
                        //" Delete from Atten_InOut where InOut_EmployeeCardNo='" + Data.EmployeeCardNo + "' and InOutDate >'" + Convert.ToDateTime(Data.LeavingDate).ToString("yyyy-MM-dd") + "' " +
                        strBuilder.Append(Employee_Admin);

                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record save successfully";
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
                                                                            "'BuklEmployeeLeft'," +
                                                                            "'" + Dns.GetHostName().ToString() + "'," +
                                                                            "GetDate()," +
                                                                            "'" + Session["EmployeeId"] + "'," +
                                                                            "'" + Session["EmployeeName"] + "'");
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                    //var GetLeavingDate = Convert.ToDateTime(Data.LeavingDate).ToString("yyyy-MM-dd");
                    //    //var GetLeavingDate =tbldata[i].LeavingDate.Convert.ToDateTime(ToString("yyyy-MM-dd"));
                    //    DapperORM.DynamicQuerySingle("Update Mas_Employee set EmployeeLeft=1 ,LeavingDate='" + GetLeavingDate + "',LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeNo='" + Data.EmployeeNo + "'");
                    //}
                    //var Message = "Record save sucessfully";
                    //var Icon = "success";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var Message = "Please select candidate";
                    var Icon = "error";
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Module_Employee_LeftEmployeeList
        public ActionResult Module_Employee_LeftEmployeeList(DailyAttendanceReportFilter OBJ)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;
                var BUDI = GetComapnyName[0].Id;
                DynamicParameters paramBranch1 = new DynamicParameters();
                paramBranch1.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch1.Add("@p_CmpId", BUDI);
                var BranchNameList = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch1).ToList();
                ViewBag.GetBranchName = BranchNameList;


                if (OBJ.CmpId != null && OBJ.BranchId == null)
                {
                    DynamicParameters paramEmployee = new DynamicParameters();
                    paramEmployee.Add("@P_Qry", "  and EmployeeBranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + OBJ.CmpId + " and UserBranchMapping.IsActive = 1) and Mas_Employee.employeeleft=1 and Mas_Employee.ContractorID <> 1");
                    var LeftEmployeeList = DapperORM.ExecuteSP<dynamic>("sp_GetEmployeeList", paramEmployee).ToList();
                    ViewBag.GetLeftEmployeeList = LeftEmployeeList;

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", OBJ.CmpId);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.GetBranchName = BranchName;
                }
                else if (OBJ.CmpId != null && OBJ.BranchId != null)
                {
                    DynamicParameters paramEmployee = new DynamicParameters();
                    paramEmployee.Add("@p_Qry", " and EmployeeBranchId=" + OBJ.BranchId + " and Mas_Employee.employeeleft=1 and mas_employee.ContractorID<>1");
                    var LeftEmployeeList = DapperORM.ExecuteSP<dynamic>("sp_GetEmployeeList", paramEmployee).ToList();
                    ViewBag.GetLeftEmployeeList = LeftEmployeeList;

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", OBJ.CmpId);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.GetBranchName = BranchName;
                }
                else if (OBJ.CmpId == null)
                {
                    //ViewBag.GetBranchName = "";
                    ViewBag.GetLeftEmployeeList = "";
                }
                return View(OBJ);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetActiveEmployee
        public ActionResult GetActiveEmployee(int? EmployeeId, string EmployeeNo, string EmployeeCardNo, int? CmpId, string BranchId, string AadhaarNo)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (BranchId != "")
                {
                    var GetEmployeeNo = DapperORM.DynamicQueryList("select count(*)as EmployeeNo from Mas_Employee where EmployeeNo= '" + EmployeeNo + "' and  Deactivate=0 and CmpID=" + CmpId + " and EmployeeBranchId=" + BranchId + "  and EmployeeId<>" + EmployeeId + "").FirstOrDefault();
                    var EmployeeNoCount = GetEmployeeNo.EmployeeNo;
                    if (EmployeeNoCount == 1)
                    {
                        var Message = "Employee no alredy exists";
                        var Icon = "error";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        //var UpdateEmployee = DapperORM.DynamicQuerySingle("update Mas_Employee Set  JoiningDate=GetDate(),LeavingDate='' where EmployeeId=" + EmployeeId + "");
                        var Message = "Record save successfully";
                        var Icon = "success";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }

                    var GetEmployeeCardNo = DapperORM.DynamicQueryList("select count(*)as EmployeeCardNo from Mas_Employee where EmployeeCardNo= '" + EmployeeNo + "' and  Deactivate=0 and CmpID=" + CmpId + " and EmployeeBranchId=" + BranchId + "  and EmployeeId<>" + EmployeeId + "").FirstOrDefault();
                    var EmployeeCardNoCount = GetEmployeeCardNo.EmployeeCardNo;
                    if (EmployeeCardNoCount == 1)
                    {
                        var Message = "Employee card no alredy exists";
                        var Icon = "error";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        //var UpdateEmployee = DapperORM.DynamicQuerySingle("update Mas_Employee Set  JoiningDate=GetDate(),LeavingDate='' where EmployeeId=" + EmployeeId + "");
                        var Message = "Record save successfully";
                        var Icon = "success";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }

                    var GetAadharNo = DapperORM.DynamicQueryList("select Count(*) as AadhaarNo from Mas_Employee_Personal where AadhaarNo='" + AadhaarNo + "' and Deactivate=0").FirstOrDefault();
                    var AadhaarNoCount = GetAadharNo.AadhaarNo;
                    if (AadhaarNoCount == 1)
                    {
                        var Message = "AadhaarNo already exists";
                        var Icon = "error";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        //var UpdateEmployee = DapperORM.DynamicQuerySingle("update Mas_Employee Set  JoiningDate=GetDate(),LeavingDate='' where EmployeeId=" + EmployeeId + "");
                        var Message = "Record save successfully";
                        var Icon = "success";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var GetEmployeeNo = DapperORM.DynamicQueryList("select count(*)as EmployeeNo from Mas_Employee where EmployeeNo= '" + EmployeeNo + "' and  Deactivate=0 and CmpID=" + CmpId + " and EmployeeId<>" + EmployeeId + "").FirstOrDefault();
                    var EmployeeNoCount = GetEmployeeNo.EmployeeNo;
                    if (EmployeeNoCount == 1)
                    {
                        var Message = "Employee no alredy exists";
                        var Icon = "error";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        ///var UpdateEmployee = DapperORM.DynamicQuerySingle("update Mas_Employee Set  JoiningDate=GetDate(),LeavingDate='' where EmployeeId="+ EmployeeId + "");
                        var Message = "Record save successfully";
                        var Icon = "success";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }

                    var GetEmployeeCardNo = DapperORM.DynamicQueryList("select count(*)as EmployeeCardNo from Mas_Employee where EmployeeCardNo= '" + EmployeeNo + "' and  Deactivate=0 and CmpID=" + CmpId + " and EmployeeId<>" + EmployeeId + "").FirstOrDefault();
                    var EmployeeCardNoCount = GetEmployeeCardNo.EmployeeCardNo;
                    if (EmployeeCardNoCount == 1)
                    {
                        var Message = "Employee card no alredy exists";
                        var Icon = "error";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        // var UpdateEmployee = DapperORM.DynamicQuerySingle("update Mas_Employee Set  JoiningDate=GetDate(),LeavingDate='' where EmployeeId=" + EmployeeId + "");
                        var Message = "Record save successfully";
                        var Icon = "success";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }

                    var GetAadharNo = DapperORM.DynamicQueryList("select Count(*) as AadhaarNo from Mas_Employee_Personal where AadhaarNo='" + AadhaarNo + "' and Deactivate=0").FirstOrDefault();
                    var AadhaarNoCount = GetAadharNo.AadhaarNo;
                    if (AadhaarNoCount == 1)
                    {
                        var Message = "AadhaarNo already exists";
                        var Icon = "error";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        //var UpdateEmployee = DapperORM.DynamicQuerySingle("update Mas_Employee Set  JoiningDate=GetDate(),LeavingDate='' where EmployeeId=" + EmployeeId + "");
                        var Message = "Record save successfully";
                        var Icon = "success";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                }
                // return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? ContractorId, int? GradeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("BulkEmployeeLeft");
                worksheet.Range(1, 1, 1, 3).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();
                var Query = "";
                if (CmpId != null)
                {
                    Query = " and Mas_Employee.CmpId=" + CmpId + " and Mas_Employee.employeeleft=0  ";
                    if (BranchId != null)
                    {
                        Query = Query + " and Mas_Employee.EmployeeBranchId=" + BranchId + "";
                    }

                    if (ContractorId != null)
                    {
                        Query = Query + " and Mas_Employee.ContractorID=" + ContractorId + "";
                    }
                    else
                    {
                        Query = Query + " and Mas_Employee.ContractorID <>1";
                    }

                    if (GradeId != null)
                    {
                        Query = Query + " and Mas_Employee.EmployeeGradeID=" + GradeId + "  order by EmployeeName";
                    }
                }
                DynamicParameters paramEmployee = new DynamicParameters();
                paramEmployee.Add("@p_Qry", Query);
                var GetEmployeeList = DapperORM.ExecuteSP<dynamic>("sp_GetLeftEmployeeList", paramEmployee).ToList();

                if (GetEmployeeList.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetEmployeeList);
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
                worksheet.Cell(1, 1).Value = "Bulk Employee Left - (" + DateTime.Now.Date.ToString("dd/MMM/yyyy") + ")";
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

        #region GetContractorName
        [HttpGet]
        public ActionResult GetContractorName(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param8 = new DynamicParameters();
                param8.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchId + " and Contractor_Master.ContractorID<>1");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param8).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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