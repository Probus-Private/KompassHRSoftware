using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_OnboardingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: Employee/AdminOnboarding
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Onboarding main View
        [HttpGet]
        public ActionResult Module_Employee_Onboarding(string EmployeeId_Encrypted, string FID_Encrypted, int? FID, int? TransferEmployeeId, int? BranchID, int? CmpId)
        {
            try
            {/*Session["OnboardCmpId"] */
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 141;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetOnboardingContractorDropdownAccess = DapperORM.DynamicQuerySingle("Select OnboardingContractorDropdownAccess from Tool_CommonTable");
                Session["OnboardingContractorDropdownAccess"] = GetOnboardingContractorDropdownAccess.OnboardingContractorDropdownAccess;

               

                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee MasEmployee = new Mas_Employee();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                var CID = GetComapnyName[0].Id;
                ViewBag.CompanyName = GetComapnyName;


                DynamicParameters param7 = new DynamicParameters();
                param7.Add("@p_employeeid", Session["EmployeeId"]);
                param7.Add("@p_CmpId", CID);
                var BranchName1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param7).ToList();

                DynamicParameters paramCategory = new DynamicParameters();
                paramCategory.Add("@query", "Select KPISubCategoryId as Id,KPISubCategoryFName as Name  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 order by Name");
                var EmployeeAllocationCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory).ToList();
                ViewBag.EmployeeAllocationCategory = EmployeeAllocationCategory;


                DynamicParameters paramCountry = new DynamicParameters();
                paramCountry.Add("@query", "Select CountryId as Id,Nationality as Name  from Mas_Country  where Deactivate=0  order by IsDefault desc");
                var CountryList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCountry).ToList();
                ViewBag.CountryList = CountryList;



                //DynamicParameters paramCountry = new DynamicParameters();
                //paramCountry.Add("@query", "Select CountryId ,Nationality as Name, IsDefault from Mas_Country  where Deactivate=0 order by Name");
                //var CountryList = DapperORM.ReturnList<Nationality>("sp_QueryExcution", paramCountry)
                //           .OrderByDescending(x => x.IsDefault)
                //           .ThenBy(x => x.Name)
                //           .ToList();

                //ViewBag.CountryList = CountryList;


                if (BranchName1.Count > 0)
                {
                    ViewBag.BranchName = BranchName1;
                    var UnitBranchId1 = BranchName1[0].Id;

                    if (GetOnboardingContractorDropdownAccess.OnboardingContractorDropdownAccess == true)
                    {
                        DynamicParameters param8 = new DynamicParameters();
                        param8.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + UnitBranchId1 + "");
                        var GetContractorDropdown1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param8).ToList();
                        ViewBag.ContractorName = GetContractorDropdown1;
                    }
                    else
                    {
                        DynamicParameters param8 = new DynamicParameters();
                        param8.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + UnitBranchId1 + " and Contractor_Master.ContractorID=1");
                        var GetContractorDropdown1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param8).ToList();
                        ViewBag.ContractorName = GetContractorDropdown1;
                    }


                    DynamicParameters paramUnit1 = new DynamicParameters();
                    paramUnit1.Add("@query", "Select  UnitId as Id, UnitName As Name from Mas_unit where Deactivate=0 and UnitBranchId= '" + UnitBranchId1 + "' order by Name");
                    var UnitName1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUnit1).ToList();
                    ViewBag.Unit = UnitName1;

                    DynamicParameters paramLine1 = new DynamicParameters();
                    paramLine1.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.BranchId='" + UnitBranchId1 + "' order by Name");
                    var List_LineMaster1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine1).ToList();
                    ViewBag.LineMasterName1 = List_LineMaster1;
                }
                else
                {
                    ViewBag.BranchName = "";
                    ViewBag.ContractorName = "";
                    ViewBag.Unit = "";
                    ViewBag.LineMasterName = "";
                }
                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select EmployeeLevelId as Id,EmployeeLevel as Name  from Mas_EmployeeLevel where Deactivate= 0 order by EmployeeLevel;
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate = 0 order by DesignationName;
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by GradeName;
                                                  select EmployeeTypeId as Id,EmployeeType as Name from mas_employeeType where Deactivate=0 order by EmployeeType;
                                                  select WageCategoryId as Id,WageCategoryName as Name from Mas_WageCategory where Deactivate=0 order by WageCategoryName;
                                                  select ZoneId as Id,ZoneName as Name from Mas_ZoneMaster where Deactivate=0 order by isdefault desc;
                                                  select CostCenterId as Id,CostCenterName as Name from Mas_CostCenter where Deactivate=0 order by isdefault desc;
                                                  select EmployeeGroupId as Id,EmployeeGroupName as Name from Mas_EmployeeGroup where Deactivate=0 order by EmployeeGroupName;
                                                  Select ProcessCategoryId as Id,ProcessCategoryName as Name from Payroll_ProcessCategory where Deactivate=0 order by isdefault desc
                                                  Select SalarySheetId as Id,SalarySheetName as Name from Payroll_SalarySheet where Deactivate=0 order by isdefault desc");
                ViewBag.DepartmentName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.EmployeeLevel = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.Designation = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.Grade = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.EmployeeType = results[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.Wage = results[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.Zone = results[6].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.CostCenterName = results[7].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.EmployeeGroupName = results[8].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.SalaryProcess = results[9].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.SalarySheet = results[10].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                //ViewBag.DepartmentName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.EmployeeLevel = results.Read<AllDropDownClass>().ToList();
                //ViewBag.Designation = results.Read<AllDropDownClass>().ToList();
                //ViewBag.Grade = results.Read<AllDropDownClass>().ToList();
                //ViewBag.EmployeeType = results.Read<AllDropDownClass>().ToList();
                //ViewBag.Wage = results.Read<AllDropDownClass>().ToList();
                //ViewBag.Zone = results.Read<AllDropDownClass>().ToList();
                //ViewBag.CostCenterName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.EmployeeGroupName = results.Read<AllDropDownClass>().ToList();

                if (Session["OnboardEmployeeId"] != null || EmployeeId_Encrypted != null)
                {
                    if (TransferEmployeeId != null)
                    {
                        Session["TransferOrigin"] = "Trans_BU";
                        Session["OnboardEmployeeId"] = TransferEmployeeId;
                    }
                    else
                    {
                        Session["TransferOrigin"] = null;
                    }

                    if (EmployeeId_Encrypted != null)
                    {
                        var GetEmployeeId = DapperORM.DynamicQueryList("Select EmployeeId from Mas_Employee where Mas_Employee.EmployeeId_Encrypted='" + EmployeeId_Encrypted + "'").FirstOrDefault();
                        Session["OnboardEmployeeId"] = GetEmployeeId?.EmployeeId;

                    }
                    ViewBag.AddUpdateTitle = "Update";
                    if (MasEmployee.EmployeeSeries != null)
                    {
                        TempData["EmployeeSeries"] = DapperORM.ListOfDynamicQueryList<dynamic>("Select EmployeeSeriesId from Mas_EmployeeSeries where  Mas_EmployeeSeries.Deactivate=0 and  Mas_EmployeeSeries.EmployeeSeriesPrefix='" + MasEmployee.EmployeeSeries + "'").ToList();
                        TempData["TempEmployeeSeries"] = MasEmployee?.EmployeeSeries;
                        TempData["EmployeeNo"] = MasEmployee?.EmployeeNo;
                        TempData["IsCriticalStageApplicable"] = MasEmployee?.IsCriticalStageApplicable;
                    }

                    DynamicParameters OnboardEMP = new DynamicParameters();
                    OnboardEMP.Add("@p_AdminEmployeeId", Session["OnboardEmployeeId"]);
                    MasEmployee = DapperORM.ReturnList<Mas_Employee>("sp_List_Mas_Employee_AdminOnboarding", OnboardEMP).FirstOrDefault();
                    ViewBag.OnboardEMP = MasEmployee;

                    //ViewBag.NoticePeriodMonthDays= DapperORM.DynamicQuerySingle($@"Select Id, MonthDays as Name from tbl_NoticePeriodType_MonthDays where Type='{MasEmployee?.NoticePeriodType}'");
                    DynamicParameters NPTdata = new DynamicParameters();
                    NPTdata.Add("@query", $@"Select Id, MonthDays as Name from tbl_NoticePeriodType_MonthDays where Type='{MasEmployee?.NoticePeriodType}'");
                    var GetNPTdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", NPTdata);
                    ViewBag.NoticePeriodMonthDays = GetNPTdata;
                    TempData["NoticePeriodMonthDays"] = MasEmployee?.NoticePeriodMonthDays;

                    DynamicParameters param6 = new DynamicParameters();
                    param6.Add("@p_employeeid", Session["EmployeeId"]);
                    if (TransferEmployeeId != null)
                    {
                        param6.Add("@p_CmpId", Session["OnboardCmpId"]);
                    }
                    else
                    {
                        param6.Add("@p_CmpId", MasEmployee.CmpID);
                    }

                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param6).ToList();
                    ViewBag.BranchName = BranchName;
                    //var UnitBranchId = BranchName[0].Id;

                    double UnitBranchId;
                    if (MasEmployee.ContractorID == null)
                    {
                        UnitBranchId = BranchName[0].Id;
                    }
                    else
                    {
                        UnitBranchId = MasEmployee.EmployeeBranchId;
                    }

                    if (GetOnboardingContractorDropdownAccess.OnboardingContractorDropdownAccess == true)
                    {
                        DynamicParameters param10 = new DynamicParameters();
                        param10.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + UnitBranchId + "");
                        var GetContractorDropdown1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param10).ToList();
                        ViewBag.ContractorName = GetContractorDropdown1;
                    }
                    else
                    {
                        DynamicParameters param11 = new DynamicParameters();
                        param11.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + UnitBranchId + " and Contractor_Master.ContractorID=1");
                        var GetContractorDropdown1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param11).ToList();
                        ViewBag.ContractorName = GetContractorDropdown1;
                    }
                    //DynamicParameters paramcmp = new DynamicParameters();
                    //paramcmp.Add("@query", "Select  BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate =0 and CmpId=" + Session["OnboardCmpId"] + " order by BranchName");
                    //var List_BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramcmp).ToList();
                    //ViewBag.BranchName = List_BranchName;                  

                    DynamicParameters paramLine = new DynamicParameters();
                    paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + MasEmployee.CmpID + " and Mas_LineMaster.BranchId=" + MasEmployee.EmployeeBranchId + " order by Name");
                    var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                    ViewBag.LineMasterName = List_LineMaster;

                    DynamicParameters paramDepartmentId = new DynamicParameters();
                    paramDepartmentId.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + MasEmployee.EmployeeDepartmentID + " order by SubDepartmentName");
                    var DepartmentName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramDepartmentId).ToList();
                    ViewBag.SubDepartmentName = DepartmentName;

                    //DynamicParameters paramCategory1 = new DynamicParameters();
                    //paramCategory1.Add("@query", "Select KPISubCategoryId as Id,KPISubCategoryFName as Name  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 and  KPISubCategoryBranchId='" + MasEmployee.EmployeeBranchId + "' order by Name");
                    //var EmployeeAllocationCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory1).ToList();
                    //ViewBag.EmployeeAllocationCategory = EmployeeAllocationCategory;

                    DynamicParameters ParamManager = new DynamicParameters();
                    ParamManager.Add("@query", @"Select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name from mas_employee where mas_employee.Deactivate=0 and EmployeeLeft=0 and mas_employee.EmployeeBranchid In(Select BranchID from UserBranchMapping where EmployeeID=" + Session["EmployeeId"] + " and IsActive=1)  order by Name");
                    var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                    ViewBag.GetManagerEmployee = Getdata;

                    DynamicParameters paramUnit = new DynamicParameters();
                    paramUnit.Add("@query", "  select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and Mas_Unit.CmpId=" + MasEmployee.CmpID + " and Mas_Unit.UnitBranchId=" + MasEmployee.EmployeeBranchId + " order by isdefault desc");
                    var UnitMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUnit).ToList();
                    ViewBag.Unit = UnitMaster;
                    TempData["ConfirmationDate"] = MasEmployee.ConfirmationDate;
                    TempData["JoiningDate"] = MasEmployee.JoiningDate;
                    //TempData["EmployeeLeft"] = MasEmployee.EmployeeLeft;
                    TempData["LeavingDate"] = MasEmployee.LeavingDate;
                    TempData["ProbationDueDate"] = MasEmployee.ProbationDueDate;
                    TempData["TraineeDueDate"] = MasEmployee.TraineeDueDate;

                    TempData["checkExisthide"] = string.IsNullOrEmpty(MasEmployee.EmployeeId_Encrypted) ? false : true;
                }

                else
                {
                    ViewBag.SubDepartmentName = "";
                    //ViewBag.Unit = "";
                    //ViewBag.LineMasterName = "";
                    //ViewBag.BranchName = "";
                    //ViewBag.ContractorName = "";
                    TempData["ConfirmationDate"] = "";
                    TempData["JoiningDate"] = "";
                    TempData["TempEmployeeSeries"] = "";
                    TempData["IsCriticalStageApplicable"] = "";
                    TempData["EmployeeLeft"] = false;
                    TempData["LeavingDate"] = "";
                    ViewBag.GetManagerEmployee = "";
                    TempData["checkExisthide"] = false;
                }

                if (FID_Encrypted != null)
                {
                    Session["PreboardFid"] = FID;
                    var PreBoardingList = DapperORM.DynamicQuerySingle("Select DocNo,FID_Encrypted,Preboarding_Mas_Employee.CmpId,Preboarding_Mas_Employee.BranchID,Preboarding_Mas_Employee.EmployeeDepartmentId,Preboarding_Mas_Employee.EmployeeDesignationId,isnull((Select EmployeeName from Mas_Employee where Deactivate=0 and  Mas_Employee.Employeeid=Preboarding_Mas_Employee.Verify_FinalApproval_Employeeid ),'') as FinalApprovalVerifierName,Mas_CompanyProfile.CompanyName,MAs_Branch.BranchName,Preboarding_Mas_Employee.EmployeeName,Mas_Department.DepartmentName,Mas_Designation.DesignationName,Preboarding_Mas_Employee.SalutationName,DocDate,FID from Preboarding_Mas_Employee  join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId=Preboarding_Mas_Employee.CmpId  join Mas_Branch on Mas_Branch.BranchId=Preboarding_Mas_Employee.BranchID  join Mas_Department on Mas_Department.DepartmentId =Preboarding_Mas_Employee.EmployeeDepartmentId join Mas_Designation on Mas_Designation.DesignationId=Preboarding_Mas_Employee.EmployeeDesignationId where Preboarding_Mas_Employee.Deactivate=0  and Mas_CompanyProfile.Deactivate=0 and Mas_Branch.deactivate=0 and FID_Encrypted='" + FID_Encrypted + "'");
                    ViewBag.PreOnboardList = PreBoardingList;
                    var DepartmentId = PreBoardingList.EmployeeDepartmentId;
                    ViewBag.PreboardFid = PreBoardingList.FID;
                    DynamicParameters param6 = new DynamicParameters();
                    param6.Add("@p_CmpId", CmpId);
                    param6.Add("@p_employeeid", Session["EmployeeId"]);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param6).ToList();
                    ViewBag.BranchName = BranchName;



                    //DynamicParameters param2 = new DynamicParameters();
                    //param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchID + " and Contractor_Master.ContractorID=1");
                    //var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();
                    //ViewBag.ContractorName = GetContractorDropdown;
                    if (GetOnboardingContractorDropdownAccess.OnboardingContractorDropdownAccess == true)
                    {
                        DynamicParameters param8 = new DynamicParameters();
                        param8.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchID + "");
                        var GetContractorDropdown1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param8).ToList();
                        ViewBag.ContractorName = GetContractorDropdown1;
                    }
                    else
                    {
                        DynamicParameters param9 = new DynamicParameters();
                        param9.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchID + " and Contractor_Master.ContractorID=1");
                        var GetContractorDropdown1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param9).ToList();
                        ViewBag.ContractorName = GetContractorDropdown1;
                    }

                    DynamicParameters paramLine = new DynamicParameters();
                    paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + BranchID + " order by Name");
                    var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                    ViewBag.LineMasterName = List_LineMaster;

                    DynamicParameters paramDepartmentId = new DynamicParameters();
                    paramDepartmentId.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + DepartmentId + " order by SubDepartmentName");
                    var DepartmentName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramDepartmentId).ToList();
                    ViewBag.SubDepartmentName = DepartmentName;

                }

                return View(MasEmployee);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsDesignationExists
        public ActionResult IsOnboardingExists(string EmployeeNo, string EmployeeId_Encrypted, int? EmployeeId, string EmployeeLeft, string EmployeeCardNo, DateTime? JoiningDate, DateTime? LeavingDate, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_EmployeeNo", EmployeeNo);
                    param.Add("@p_EmployeeCardNo", EmployeeCardNo);
                    param.Add("@p_EmployeeId", EmployeeId);
                    param.Add("@p_EmployeeBranchId", BranchId);
                    param.Add("@p_JoiningDate", JoiningDate);
                    param.Add("@p_EmployeeLeft", EmployeeLeft);
                    param.Add("@p_LeavingDate", LeavingDate);
                    param.Add("@p_EmployeeId_Encrypted", EmployeeId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee", param);
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
        public ActionResult SaveUpdate(Mas_Employee Masemployee)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(Masemployee.EmployeeId_Encrypted) ? "Save" : "Update");
                param.Add("@p_EmployeeId", Masemployee.EmployeeId);
                param.Add("@p_EmployeeId_Encrypted", Masemployee.EmployeeId_Encrypted);
                param.Add("@p_PreboardingFid", Masemployee.PreboardingFid);
                //if (Session["PreboardFid"] != null)
                //{
                //    param.Add("@p_PreboardingFid", Masemployee.PreboardingFid);
                //}
                //else
                //{
                //    param.Add("@p_PreboardingFid", 0);
                //}

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                if (Session["TransferOrigin"] != null)
                {
                    param.Add("@p_EmployeeBranchId", Session["OnboardBranchId"]);
                    param.Add("@p_CmpID", Session["OnboardCmpId"]);
                }
                else
                {
                    param.Add("@p_EmployeeBranchId", Masemployee.EmployeeBranchId);
                    param.Add("@p_CmpID", Masemployee.CmpID);
                }
                //param.Add("@p_EmployeeBranchId", Masemployee.EmployeeBranchId);
                param.Add("@p_EmployeeSeries", Masemployee.EmployeeSeries);
                param.Add("@p_EmployeeNo", Masemployee.EmployeeNo);
                param.Add("@p_EmployeeCardNo", Masemployee.EmployeeCardNo);
                param.Add("@p_LocalExpat", Masemployee.LocalExpat);
                param.Add("@p_IsNRI", Masemployee.IsNRI);
                param.Add("@p_Salutation", Masemployee.Salutation);
                param.Add("@p_EmployeeName", Masemployee.EmployeeName);
                param.Add("@p_EmployeeLevelID", Masemployee.EmployeeLevelID);
                param.Add("@p_EmployeeWageID", Masemployee.EmployeeWageID);
                param.Add("@p_EmployeeDepartmentID", Masemployee.EmployeeDepartmentID);
                param.Add("@p_EmployeeSubDepartmentName", Masemployee.EmployeeSubDepartmentName);
                param.Add("@p_EmployeeDesignationID", Masemployee.EmployeeDesignationID);
                param.Add("@p_EmployeeTypeID", Masemployee.EmployeeTypeID);
                param.Add("@p_EmployeeGradeID", Masemployee.EmployeeGradeID);
                param.Add("@p_EmployeeGroupID", Masemployee.EmployeeGroupID);
                param.Add("@p_EmployeeCostCenterID", Masemployee.EmployeeCostCenterID);
                param.Add("@p_EmployeeZoneID", Masemployee.EmployeeZoneID);
                param.Add("@p_EmployeeUnitID", Masemployee.EmployeeUnitID);
                param.Add("@p_ContractorID", Masemployee.ContractorID);
                param.Add("@p_JoiningDate", Masemployee.JoiningDate);
                param.Add("@p_IsJoiningSpecial", Masemployee.IsJoiningSpecial);
                param.Add("@p_JoiningStatus", Masemployee.JoiningStatus);
                param.Add("@p_TraineeDueDate", Masemployee.TraineeDueDate);
                param.Add("@p_ProbationDueDate", Masemployee.ProbationDueDate);
                param.Add("@p_ConfirmationDate", Masemployee.ConfirmationDate);
                param.Add("@p_IsConfirmation", Masemployee.IsConfirmation);
                param.Add("@p_ConfirmationBy", Masemployee.ConfirmationBy);
                param.Add("@p_CompanyMobileNo", Masemployee.CompanyMobileNo);
                param.Add("@p_CompanyMailID", Masemployee.CompanyMailID);
                param.Add("@p_EmployeeLeft", Masemployee.EmployeeLeft);
                if (Masemployee.EmployeeLeft == false)
                {
                    param.Add("@p_LeavingDate", null);
                }
                else
                {
                    param.Add("@p_LeavingDate", Masemployee.LeavingDate);
                    param.Add("@p_LeftEmployeeApprovedBy", Session["EmployeeName"]);
                }
                param.Add("@p_LeavingReason", Masemployee.LeavingReason);
                param.Add("@p_EM_Atten_DailyMonthly", Masemployee.EM_Atten_DailyMonthly);
                param.Add("@p_EmployeeLineID", Masemployee.EmployeeLineID);
                param.Add("@p_EmployeeAllocationCategoryId", Masemployee.EmployeeAllocationCategoryId);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_EM_SalarySheetId", Masemployee.EM_SalarySheetId);
                param.Add("@p_EM_ProcessCategoryId", Masemployee.EM_ProcessCategoryId);

                param.Add("@p_NoticePeriodType", Masemployee.NoticePeriodType);
                param.Add("@p_NoticePeriodMonthDays", Masemployee.NoticePeriodMonthDays);
                param.Add("@p_EmployeeCountryId", Masemployee.EmployeeCountryId);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                DapperORM.ExecuteReturn("sp_SUD_Mas_Employee", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["PId"] = param.Get<string>("@p_Id");
                // 3. AFTER SAVE: Check if ResourceRequest should be Closed
                if (Session["PreboardFid"] != null)
                {
                    var preboardFid = Convert.ToInt32(Session["PreboardFid"]);

                    // Find the ResourceId linked to this PreboardingFid
                    var resource = DapperORM.QuerySingle(@"
        SELECT TOP 1 rrr.ResourceId, rrr.TotalPositions
        FROM Recruitment_ResourceRequest rrr
        JOIN Recruitment_Resume rr ON rrr.ResourceId = rr.Resume_ResourceId
        JOIN Preboarding_Mas_Employee pre ON rr.PreboardingId = pre.Fid
        WHERE pre.Fid = '" + preboardFid + "'");

                    if (resource != null)
                    {
                        int resourceId = Convert.ToInt32(resource.ResourceId);
                        int totalPositions = Convert.ToInt32(resource.TotalPositions);

                        // Get onboarded count
                        var onboarded = DapperORM.QuerySingle(@"
            SELECT COUNT(DISTINCT emp.EmployeeId) AS OnboardedCount
            FROM Recruitment_ResourceRequest rrr
            JOIN Recruitment_Resume rr ON rrr.ResourceId = rr.Resume_ResourceId
            JOIN Preboarding_Mas_Employee pre ON rr.PreboardingId = pre.Fid
            LEFT JOIN Mas_Employee emp ON emp.PreboardingFid = pre.DocNo
            WHERE rrr.ResourceId = @ResourceId",
                            new { ResourceId = resourceId });

                        int onboardedCount = onboarded?.OnboardedCount ?? 0;

                        // Close the resource request if filled
                        if (onboardedCount >= totalPositions)
                        {
                            DapperORM.Execute(
                                @"UPDATE Recruitment_ResourceRequest 
                  SET ResourceRequestStatus = 'Closed' 
                  WHERE ResourceId = @ResourceId",
                                new { ResourceId = resourceId });
                        }
                    }
                }
                return RedirectToAction("Module_Employee_Onboarding", "Module_Employee_Onboarding");

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
                param.Add("@p_EmployeeId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Mas_Employee", param);
                ViewBag.AdminOnboardingGetList = data;
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
        public ActionResult Delete(string EmployeeId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EmployeeId_Encrypted", EmployeeId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Employee_Onboarding");
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
        public ActionResult GetBusinessUnit(int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                var UnitBranchId = BranchName[0].Id;

                DynamicParameters paramunit = new DynamicParameters();
                paramunit.Add("@query", "Select  UnitId as Id, UnitName As Name from Mas_unit where Deactivate=0 and CmpId= '" + CmpId + "' and UnitBranchId= '" + UnitBranchId + "'");
                var UnitName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramunit).ToList();

                DynamicParameters paramLine = new DynamicParameters();
                paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + UnitBranchId + " order by Name");
                var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                DynamicParameters paramCategory = new DynamicParameters();
                paramCategory.Add("@query", "Select KPISubCategoryId as Id,KPISubCategoryFName as Name  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0  and CmpID=" + CmpId + " order by Name ");
                var EmployeeAllocationCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory).ToList();

                return Json(new { BranchName = BranchName, UnitName = UnitName, List_LineMaster = List_LineMaster, EmployeeAllocationCategory = EmployeeAllocationCategory }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetUnit
        [HttpGet]
        public ActionResult GetUnit(int? CmpId, int? UnitBranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select  UnitId as Id, UnitName As Name from Mas_unit where Deactivate=0 and CmpId= '" + CmpId + "' and UnitBranchId= '" + UnitBranchId + "'");
                var UnitName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                DynamicParameters paramLine = new DynamicParameters();
                paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + UnitBranchId + " order by Name");
                var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                string Getp_qry = "";
                if (Convert.ToBoolean(Session["OnboardingContractorDropdownAccess"]) == true)
                {
                    Getp_qry = " and Mas_ContractorMapping.BranchID=" + UnitBranchId + "";
                }
                else
                {
                    Getp_qry = " and Mas_ContractorMapping.BranchID=" + UnitBranchId + " and Contractor_Master.ContractorID=1";
                }

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_qry", Getp_qry);
                var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();

                //DynamicParameters paramCategory = new DynamicParameters();
                //paramCategory.Add("@query", "Select KPISubCategoryId as Id,KPISubCategoryFName as Name  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 and  KPISubCategoryBranchId='" + UnitBranchId + "' order by Name");
                //var EmployeeAllocationCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory).ToList();
                //// var GetDesignationName = "";
                //if (EmployeeAllocationCategory.Count > 0)
                //{
                //    var AllocationCategoryId = EmployeeAllocationCategory[0].Id;
                //    DynamicParameters paramCategory2 = new DynamicParameters();
                //    paramCategory2.Add("@query", "Select DesignationId as Id,DesignationName as Name from Mas_Designation,KPI_CategoryDepartment,KPI_SubCategory where Mas_Designation.Deactivate=0 and KPI_SubCategory.Deactivate=0 and KPI_SubCategory.KPISubCategoryId=KPI_CategoryDepartment.KPI_Category_CategoryId and Mas_Designation.DesignationId=KPI_CategoryDepartment.KPI_Category_DesignationId and KPI_CategoryDepartment.KPI_Category_CategoryId='" + AllocationCategoryId + "'");
                //    var GetDesignationName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory2).ToList();
                //    return Json(new { UnitName = UnitName, List_LineMaster = List_LineMaster, GetContractorDropdown = GetContractorDropdown, EmployeeAllocationCategory = EmployeeAllocationCategory, GetDesignationName = GetDesignationName }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    var GetDesignationName = "";
                //    return Json(new { UnitName = UnitName, List_LineMaster = List_LineMaster, GetContractorDropdown = GetContractorDropdown, EmployeeAllocationCategory = EmployeeAllocationCategory, GetDesignationName = GetDesignationName }, JsonRequestBehavior.AllowGet);

                //}
                return Json(new { UnitName = UnitName, List_LineMaster = List_LineMaster, GetContractorDropdown = GetContractorDropdown }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployeeSeriesNo
        [HttpGet]
        public ActionResult GetEmployeeSeriesNo(int? EmployeeSeriesID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var GetEmployeeNo = "select top 1 RIGHT('000' +cast(EmployeeNo +1 as nvarchar(8)),4) as EmployeeNo FROM Mas_Employee,Mas_EmployeeSeries WHERE Mas_EmployeeSeries.Deactivate = 0and  Mas_Employee.Deactivate = 0  and Mas_Employee.EmployeeSeries=Mas_EmployeeSeries.EmployeeSeriesPrefix  and Mas_Employee.EmployeeSeries=(select EmployeeSeriesPrefix 	from Mas_EmployeeSeries where Mas_EmployeeSeries.Deactivate=0 and  Mas_EmployeeSeries.EmployeeSeriesId=" + EmployeeSeriesID + ")";
                var EmployeeNo = DapperORM.DynamicQuerySingle(GetEmployeeNo);

                var GetEmployeeSeriesPrefix = "Select EmployeeSeriesPrefix from Mas_EmployeeSeries where Mas_EmployeeSeries.Deactivate=0 and  Mas_EmployeeSeries.EmployeeSeriesId=" + EmployeeSeriesID + "";
                var EmployeeSeriesPrefix = DapperORM.DynamicQuerySingle(GetEmployeeSeriesPrefix);

                //DynamicParameters param = new DynamicParameters();
                //param.Add("@query", "Select Isnull(Max(Mas_Employee.EmployeeNo),0)+1 As EmployeeNo from Mas_Employee WHERE Mas_Employee.Deactivate =0 and  Mas_Employee.EmployeeSeries=(select EmployeeSeriesPrefix from Mas_EmployeeSeries where Mas_EmployeeSeries.Deactivate=0 and  Mas_EmployeeSeries.EmployeeSeriesId="+ EmployeeSeriesID + ")");
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(new { EmployeeNo = EmployeeNo, EmployeeSeriesPrefix = EmployeeSeriesPrefix }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetSubDepartment
        [HttpGet]
        public ActionResult GetSubDepartment(int? DepartmentId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + DepartmentId + "");
                var Data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region StaffLeftEmployeeList
        public ActionResult StaffLeftEmployeeList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 379;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                DynamicParameters paramEmployee = new DynamicParameters();
                paramEmployee.Add("@P_Qry", " and EmployeeBranchId in (select BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and UserBranchMapping.IsActive = 1) and Mas_Employee.employeeleft=1 and mas_employee.ContractorID=1");
                var LeftEmployeeList = DapperORM.ExecuteSP<dynamic>("sp_GetEmployeeList", paramEmployee).ToList();
                ViewBag.GetLeftEmployeeList = LeftEmployeeList;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        //#region GetDesignationDropDown
        //[HttpGet]
        //public ActionResult GetDesignationDropDown(int? AllocationCategoryId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }
        //        DynamicParameters paramCategory = new DynamicParameters();
        //        paramCategory.Add("@query", "Select DesignationId as Id,DesignationName as Name from Mas_Designation,KPI_CategoryDepartment,KPI_SubCategory where Mas_Designation.Deactivate=0 and KPI_SubCategory.Deactivate=0 and KPI_SubCategory.KPISubCategoryId=KPI_CategoryDepartment.KPI_Category_CategoryId and Mas_Designation.DesignationId=KPI_CategoryDepartment.KPI_Category_DesignationId and KPI_CategoryDepartment.KPI_Category_CategoryId=" + AllocationCategoryId + "");
        //        var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory).ToList();
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion

        #region UpdateReportingManager
        public ActionResult UpdateReportingManager(int? EmployeeId, int? ddlManagerId)
        {
            try
            {
                DapperORM.DynamicQuerySingle("update Mas_Employee_Reporting Set ReportingManager1=" + ddlManagerId + ",ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GetDate()  where Mas_Employee_Reporting.Deactivate=0 and ReportingManager1=" + Session["OnboardEmployeeId"] + "");
                TempData["Message"] = "Reporting manager Changed";
                TempData["Icon"] = "success";
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            //return View();
        }
        #endregion



        #region
        [HttpGet]
        public ActionResult GetNoticePeriodMonthDays(string NoticePeriodType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //ViewBag.NoticePeriodMonthDays = DapperORM.DynamicQuerySingle("Select Type Id, MonthDays as Name from tbl_NoticePeriodType_MonthDays where Type={NoticePeriodType}");
                DynamicParameters MonthDays = new DynamicParameters();
                MonthDays.Add("@query", $@"Select Id, MonthDays as Name from tbl_NoticePeriodType_MonthDays where Type='{NoticePeriodType}'");
                var NoticePeriodMonthDays = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", MonthDays).ToList();
                return Json(new { NoticePeriodMonthDays = NoticePeriodMonthDays }, JsonRequestBehavior.AllowGet);
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