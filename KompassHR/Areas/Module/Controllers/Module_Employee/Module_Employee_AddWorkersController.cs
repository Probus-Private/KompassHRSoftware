using Dapper;
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

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_AddWorkersController : Controller
    {
        // GET: Module/Module_Employee_AddWorkers
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        GetMenuList ClsGetMenuList = new GetMenuList();
        #region Module_Employee_AddWorkers
        public ActionResult Module_Employee_AddWorkers(string EmployeeId_Encrypted, int? EmployeeId, string GetUpdate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 298;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                Mas_AddEmployeeWorker AddEmployeeWorker = new Mas_AddEmployeeWorker();

                //-------------------Admin Infromation Dropdown---------------------------------------------------------------------------------------------------------------------
                DynamicParameters paramCompany = new DynamicParameters();
                var results = DapperORM.DynamicQueryMultiple(@"                                                
                                                  select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                   select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate = 0 and ApplicableForWorker = 1 order by DesignationName
                                                  Select CriticalStageId as Id,CriticalStageName As Name from Mas_CriticalStageCategory where Mas_CriticalStageCategory.Deactivate=0 order by Name
                                                  Select GradeId as Id ,GradeName As Name from Mas_Grade where Deactivate=0 order by Name
                                                  Select EmployeeLevelId as Id ,EmployeeLevel As Name from Mas_EmployeeLevel where Deactivate=0 order by Name 
                                                  Select QualificationPFId as Id ,QualificationPFName As Name from Mas_Qualification_PF where Deactivate=0 order by Name;
                                                  Select KPISubCategoryId as Id,KPISubCategoryFName as Name  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 order by Name");
                ViewBag.DepartmentName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.Designation = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.CriticalStageName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeName = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.EmployeeLevel = results[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.QualificationName = results[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.EmployeeAllocationCategory = results[6].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                //select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate = 0 and ApplicableForWorker = 1 order by DesignationName
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_employeeid", Session["EmployeeId"]);
                param1.Add("@p_CmpId", CMPID);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param1).ToList();
                if (data.Count > 0)
                {
                    ViewBag.BranchName = data;
                    var BUID = data[0].Id;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BUID + " and Contractor_Master.ContractorID<>1");
                    var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();
                    ViewBag.ContractorName = GetContractorDropdown;

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@query", "Select  UnitId as Id, UnitName As Name from Mas_unit where Deactivate=0 and CmpId= '" + CMPID + "' and UnitBranchId= '" + BUID + "'order by IsDefault desc");
                    var Masunit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                    ViewBag.Unit = Masunit;

                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@query", "Select LineId as Id, LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate = 0 and Mas_LineMaster.CmpId = '" + CMPID + "' and Mas_LineMaster.BranchId = '" + BUID + "'");
                    var MasLineMasterit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();
                    ViewBag.LineMasterName = MasLineMasterit;

                    DynamicParameters param5 = new DynamicParameters();
                    param5.Add("@query", "Select ShiftGroupId As Id, ShiftGroupFName As Name from Atten_ShiftGroups Where Deactivate = 0 and CmpID = '" + CMPID + "' and ShiftGroupBranchId = '" + BUID + "'");
                    var AttenShiftGroups = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param5).ToList();
                    ViewBag.GetShiftGroupsName = AttenShiftGroups;

                    DynamicParameters param6 = new DynamicParameters();
                    param6.Add("@query", "Select ShiftRuleId As Id, ShiftRuleName As Name from Atten_ShiftRule Where Deactivate = 0 and CmpId = " + CMPID + " and ShiftRuleBranchId =' " + BUID + "'");
                    var AttenShiftRule = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param6).ToList();
                    ViewBag.GetShiftRuleName = AttenShiftRule;

                    //DynamicParameters paramCategory = new DynamicParameters();
                    //paramCategory.Add("@query", "Select KPISubCategoryId as Id,KPISubCategoryFName as Name  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 and  KPISubCategoryBranchId='" + BUID + "' order by Name");
                    //var EmployeeAllocationCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory).ToList();
                    //ViewBag.EmployeeAllocationCategory = EmployeeAllocationCategory;
                    //if (EmployeeAllocationCategory.Count > 0)
                    //{
                    //    var EmployeeCategoryid = EmployeeAllocationCategory[0].Id;
                    //    ViewBag.EmployeeAllocationCategory = EmployeeAllocationCategory;
                    //    DynamicParameters paramCategory1 = new DynamicParameters();
                    //    paramCategory1.Add("@query", "Select DesignationId as Id,DesignationName as Name from Mas_Designation,KPI_CategoryDepartment,KPI_SubCategory where Mas_Designation.Deactivate=0 and KPI_SubCategory.Deactivate=0 and KPI_SubCategory.KPISubCategoryId=KPI_CategoryDepartment.KPI_Category_CategoryId and Mas_Designation.DesignationId=KPI_CategoryDepartment.KPI_Category_DesignationId and KPI_CategoryDepartment.KPI_Category_CategoryId=" + EmployeeCategoryid + "");
                    //    ViewBag.Designation = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory1).ToList();
                    //}
                    //else
                    //{
                    //    ViewBag.Designation = "";
                    //    ViewBag.EmployeeAllocationCategory = "";
                    //}
                }
                else
                {
                    ViewBag.BranchName = "";
                    ViewBag.ContractorName = "";
                    ViewBag.Unit = "";
                    ViewBag.LineMasterName = "";
                }

                ViewBag.SubDepartmentName = "";
                // ViewBag.Unit = "";
                //ViewBag.LineMasterName = "";
                //ViewBag.BranchName = "";
                //ViewBag.GetShiftGroupsName = "";
                //ViewBag.GetShiftRuleName = "";
                TempData["GetBirthdayDate"] = "";
                TempData["JoiningDate"] = "";
                TempData["EmployeeCriticalStageID"] = "";




                //-------------------Admin Infromation Dropdown---------------------------------------------------------------------------------------------------------------------

                if (EmployeeId_Encrypted != null || Session["WorkerOnboardEmployeeId"] != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    if (Session["WorkerOnboardEmployeeId"] != null)
                    {
                        param.Add("@p_EmployeeId", Session["WorkerOnboardEmployeeId"]);
                    }
                    else
                    {
                        param.Add("@p_EmployeeId", EmployeeId);
                    }
                    param.Add("@p_EmployeeId_Encrypted", EmployeeId_Encrypted);
                    AddEmployeeWorker = DapperORM.ReturnList<Mas_AddEmployeeWorker>("sp_List_Mas_ActiveWorker", param).FirstOrDefault();
                    //if (AddEmployeeWorker.EM_Atten_DailyMonthly==null)
                    //{
                    //    AddEmployeeWorker.EM_Atten_DailyMonthly = "Daily";
                    //}

                    var objCmpID = AddEmployeeWorker.CmpID;
                    var objEmployeeBranchId = AddEmployeeWorker.EmployeeBranchId;
                    var objEmployeeDepartmentID = AddEmployeeWorker.EmployeeDepartmentID;
                    TempData["GetBirthdayDate"] = AddEmployeeWorker.BirthdayDate;
                    TempData["JoiningDate"] = AddEmployeeWorker.JoiningDate;
                    TempData["LeavingDate"] = AddEmployeeWorker.LeavingDate;
                    TempData["EmployeeLeft"] = AddEmployeeWorker.EmployeeLeft;
                    TempData["EmployeeCriticalStageID"] = AddEmployeeWorker.EmployeeCriticalStageID;
                    TempData["GetUpdate"] = GetUpdate;

                    DynamicParameters paramDPT = new DynamicParameters();
                    paramDPT.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + AddEmployeeWorker.EmployeeDepartmentID + "");
                    var GetSubDepartmentName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramDPT).ToList();
                    ViewBag.SubDepartmentName = GetSubDepartmentName;

                    DynamicParameters paramUNIT = new DynamicParameters();
                    paramUNIT.Add("@query", "Select  UnitId as Id, UnitName As Name from Mas_unit where Deactivate=0 and CmpId= '" + objCmpID + "' and UnitBranchId= '" + objEmployeeBranchId + "'order by IsDefault desc");
                    var GetUnitName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUNIT).ToList();
                    ViewBag.Unit = GetUnitName;

                    DynamicParameters paramLine = new DynamicParameters();
                    paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + objCmpID + " and Mas_LineMaster.BranchId=" + objEmployeeBranchId + " order by Name");
                    var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                    ViewBag.LineMasterName = List_LineMaster;

                    DynamicParameters paramShiftGroupFName = new DynamicParameters();
                    paramShiftGroupFName.Add("@query", "Select ShiftGroupId As Id,ShiftGroupFName As Name from Atten_ShiftGroups Where Deactivate=0 and CmpID=" + objCmpID + " and ShiftGroupBranchId=" + objEmployeeBranchId + "");
                    var ShiftGroupFName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShiftGroupFName).ToList();
                    ViewBag.GetShiftGroupsName = ShiftGroupFName;

                    DynamicParameters paramShiftRuleName = new DynamicParameters();
                    paramShiftRuleName.Add("@query", "Select ShiftRuleId As Id,ShiftRuleName As Name from Atten_ShiftRule Where Deactivate=0 and CmpId=" + objCmpID + " and ShiftRuleBranchId=" + objEmployeeBranchId + "");
                    var ShiftRuleName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShiftRuleName).ToList();
                    ViewBag.GetShiftRuleName = ShiftRuleName;

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", objCmpID);
                    var GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.BranchName = GetBranchName;

                    DynamicParameters param9 = new DynamicParameters();
                    param9.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + AddEmployeeWorker.EmployeeBranchId + " and Contractor_Master.ContractorID<>1");
                    var GetContractorDropdown1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param9).ToList();
                    ViewBag.ContractorName = GetContractorDropdown1;

                    var result = DapperORM.DynamicQuerySingle(@"SELECT IsCanteenApplicable FROM Tool_CommonTable");
                    TempData["GetIsCanteenApplicable"] = result.IsCanteenApplicable;
                    //if (EmployeeAllocationCategory.Count > 0)
                    //{
                    //    var EmployeeCategoryid = EmployeeAllocationCategory[0].Id;
                    //    ViewBag.EmployeeAllocationCategory = EmployeeAllocationCategory;
                    //    DynamicParameters paramCategory1 = new DynamicParameters();
                    //    paramCategory1.Add("@query", "Select DesignationId as Id,DesignationName as Name from Mas_Designation,KPI_CategoryDepartment,KPI_SubCategory where Mas_Designation.Deactivate=0 and KPI_SubCategory.Deactivate=0 and KPI_SubCategory.KPISubCategoryId=KPI_CategoryDepartment.KPI_Category_CategoryId and Mas_Designation.DesignationId=KPI_CategoryDepartment.KPI_Category_DesignationId and KPI_CategoryDepartment.KPI_Category_CategoryId=" + AddEmployeeWorker.EmployeeAllocationCategoryId + "");
                    //    ViewBag.Designation = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory1).ToList();
                    //}
                }
                return View(AddEmployeeWorker);

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
                var BusinessUnit = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                var BUID = BusinessUnit[0].Id;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BUID + " and Contractor_Master.ContractorID<>1");
                var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();
                //ViewBag.ContractorName = GetContractorDropdown;

                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@query", "Select  UnitId as Id, UnitName As Name from Mas_unit where Deactivate=0 and CmpId= " + CmpId + " and UnitBranchId= " + BUID + " order by IsDefault desc");
                var Masunit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                //ViewBag.Unit = Masunit;

                DynamicParameters param4 = new DynamicParameters();
                param4.Add("@query", "Select LineId as Id, LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate = 0 and Mas_LineMaster.CmpId = " + CmpId + " and Mas_LineMaster.BranchId = " + BUID + "");
                var MasLineMasterit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();
                //ViewBag.LineMasterName = MasLineMasterit;

                DynamicParameters param5 = new DynamicParameters();
                param5.Add("@query", "Select ShiftGroupId As Id, ShiftGroupFName As Name from Atten_ShiftGroups Where Deactivate = 0 and CmpID = " + CmpId + " and ShiftGroupBranchId = " + BUID + "");
                var AttenShiftGroups = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param5).ToList();
                //ViewBag.GetShiftGroupsName = AttenShiftGroups;

                DynamicParameters param6 = new DynamicParameters();
                param6.Add("@query", "Select ShiftRuleId As Id, ShiftRuleName As Name from Atten_ShiftRule Where Deactivate = 0 and CmpId = " + CmpId + " and ShiftRuleBranchId =' " + BUID + "'");
                var AttenShiftRule = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param6).ToList();
                // ViewBag.GetShiftRuleName = AttenShiftRule;

                    //ViewBag.EmployeeAllocationCategory = EmployeeAllocationCategory;
                //if (data != null)
                //{
                //    Session["GetId"] = data[0].Id;
                //}
                //else
                //{
                //    Session["GetId"] = null;
                //}
                return Json(new { BusinessUnit = BusinessUnit, GetContractorDropdown = GetContractorDropdown, Masunit = Masunit, MasLineMasterit = MasLineMasterit, AttenShiftGroups = AttenShiftGroups, AttenShiftRule = AttenShiftRule}, JsonRequestBehavior.AllowGet);
                //return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetDropdownValue
        [HttpGet]
        public ActionResult GetDropdownValue(int? CmpId, int? BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (BranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "Select  UnitId as Id, UnitName As Name from Mas_unit where Deactivate=0 and CmpId= '" + CmpId + "' and UnitBranchId= '" + BranchId + "'order by IsDefault desc");
                    var UnitName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                    DynamicParameters paramLine = new DynamicParameters();
                    paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + BranchId + " order by Name");
                    var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                    DynamicParameters paramShiftGroupFName = new DynamicParameters();
                    paramShiftGroupFName.Add("@query", "Select ShiftGroupId As Id,ShiftGroupFName As Name from Atten_ShiftGroups Where Deactivate=0 and CmpID=" + CmpId + " and ShiftGroupBranchId=" + BranchId + " order by IsDefault desc");
                    var ShiftGroupFName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShiftGroupFName).ToList();

                    DynamicParameters paramShiftRuleName = new DynamicParameters();
                    paramShiftRuleName.Add("@query", "Select ShiftRuleId As Id,ShiftRuleName As Name from Atten_ShiftRule Where Deactivate=0 and CmpId=" + CmpId + " and ShiftRuleBranchId=" + BranchId + " order  by IsDefault desc");
                    var ShiftRuleName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShiftRuleName).ToList();

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchId + " and Contractor_Master.ContractorID<>1");
                    var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();

                    //DynamicParameters paramCategory = new DynamicParameters();
                    //paramCategory.Add("@query", "Select KPISubCategoryId as Id,KPISubCategoryFName as Name  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 and  KPISubCategoryBranchId='" + BranchId + "' order by Name");
                    //var EmployeeAllocationCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory).ToList();
                    // var GetDesignationName = "";
                    //if (EmployeeAllocationCategory.Count > 0)
                    //{
                    //    var AllocationCategoryId = EmployeeAllocationCategory[0].Id;
                    //    DynamicParameters paramCategory2 = new DynamicParameters();
                    //    paramCategory2.Add("@query", "Select DesignationId as Id,DesignationName as Name from Mas_Designation,KPI_CategoryDepartment,KPI_SubCategory where Mas_Designation.Deactivate=0 and KPI_SubCategory.Deactivate=0 and KPI_SubCategory.KPISubCategoryId=KPI_CategoryDepartment.KPI_Category_CategoryId and Mas_Designation.DesignationId=KPI_CategoryDepartment.KPI_Category_DesignationId and KPI_CategoryDepartment.KPI_Category_CategoryId='" + AllocationCategoryId + "'");
                    //    var GetDesignationName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory2).ToList();
                    //    return Json(new { UnitName = UnitName, List_LineMaster = List_LineMaster, ShiftGroupFName = ShiftGroupFName, ShiftRuleName = ShiftRuleName, GetContractorDropdown = GetContractorDropdown, GetDesignationName= GetDesignationName , EmployeeAllocationCategory = EmployeeAllocationCategory }, JsonRequestBehavior.AllowGet);
                    //}
                    //else
                    //{
                    //    var GetDesignationName = "";
                    //    return Json(new { UnitName = UnitName, List_LineMaster = List_LineMaster, ShiftGroupFName = ShiftGroupFName, ShiftRuleName = ShiftRuleName, GetContractorDropdown = GetContractorDropdown, GetDesignationName = GetDesignationName, EmployeeAllocationCategory = EmployeeAllocationCategory }, JsonRequestBehavior.AllowGet);
                    //}

                    return Json(new { UnitName = UnitName, List_LineMaster = List_LineMaster, ShiftGroupFName = ShiftGroupFName, ShiftRuleName = ShiftRuleName, GetContractorDropdown = GetContractorDropdown }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "Select  UnitId as Id, UnitName As Name from Mas_unit where Deactivate=0 and CmpId= '" + CmpId + "' and UnitBranchId= '" + Session["GetId"] + "'order by IsDefault desc");
                    var UnitName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                    DynamicParameters paramLine = new DynamicParameters();
                    paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + Session["GetId"] + " order by Name");
                    var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                    DynamicParameters paramShiftGroupFName = new DynamicParameters();
                    paramShiftGroupFName.Add("@query", "Select ShiftGroupId As Id,ShiftGroupFName As Name from Atten_ShiftGroups Where Deactivate=0 and CmpID=" + CmpId + " and ShiftGroupBranchId=" + Session["GetId"] + " order by IsDefault desc");
                    var ShiftGroupFName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShiftGroupFName).ToList();

                    DynamicParameters paramShiftRuleName = new DynamicParameters();
                    paramShiftRuleName.Add("@query", "Select ShiftRuleId As Id,ShiftRuleName As Name from Atten_ShiftRule Where Deactivate=0 and CmpId=" + CmpId + " and ShiftRuleBranchId=" + Session["GetId"] + " order  by IsDefault desc");
                    var ShiftRuleName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShiftRuleName).ToList();

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@p_branchid", Session["GetId"]);
                    var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param3).ToList();
                    return Json(new { UnitName = UnitName, List_LineMaster = List_LineMaster, ShiftGroupFName = ShiftGroupFName, ShiftRuleName = ShiftRuleName, GetContractorDropdown = GetContractorDropdown }, JsonRequestBehavior.AllowGet);
                }

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
        public ActionResult IsAddworkerExists(string EmployeeCardNo, string AadhaarNo, string EmployeeNo, string CmpId, string BranchId, string PersonalId_Encrypted, string EmployeeId_Encrypted, string txtEmployeeId)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PersonalId_Encrypted", PersonalId_Encrypted);
                param.Add("@p_EmployeeId_Encrypted", EmployeeId_Encrypted);
                param.Add("@p_EmployeeCardNo", EmployeeCardNo);
                param.Add("@p_EmployeeNo", EmployeeNo);
                param.Add("@p_EmployeeId", txtEmployeeId);
                param.Add("@p_AadhaarNo", AadhaarNo);
                param.Add("@p_CmpID", CmpId);
                param.Add("@p_EmployeeBranchId", BranchId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Aadhar", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_UpdateCase", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_EmployeeWorkers", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                var Aadhar = param.Get<string>("@p_Aadhar");
                var UpdateCase = param.Get<string>("@p_UpdateCase");
                if (Message != "")
                {
                    return Json(new { Message, Icon, Aadhar, UpdateCase }, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate(Mas_AddEmployeeWorker AddEmployeeWorker)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 298;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (AddEmployeeWorker.EmployeeLeft == true)
                {
                    param.Add("@p_process", "Rejoin");
                }
                else
                {
                    param.Add("@p_process", string.IsNullOrEmpty(AddEmployeeWorker.EmployeeId_Encrypted) ? "Save" : "Update");
                }

                param.Add("@p_EmployeeId", AddEmployeeWorker.EmployeeId);
                param.Add("@p_EmployeeId_Encrypted", AddEmployeeWorker.EmployeeId_Encrypted);
                param.Add("@p_CmpID", AddEmployeeWorker.CmpID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_EmployeeBranchId", AddEmployeeWorker.EmployeeBranchId);
                param.Add("@p_EmployeeNo", AddEmployeeWorker.EmployeeNo);
                param.Add("@p_EmployeeCardNo", AddEmployeeWorker.EmployeeCardNo);
                param.Add("@p_Salutation", AddEmployeeWorker.Salutation);
                param.Add("@p_EmployeeName", AddEmployeeWorker.EmployeeName);
                param.Add("@p_EmployeeDepartmentID", AddEmployeeWorker.EmployeeDepartmentID);
                param.Add("@p_EmployeeSubDepartmentName", AddEmployeeWorker.EmployeeSubDepartmentName);
                param.Add("@p_EmployeeDesignationID", AddEmployeeWorker.EmployeeDesignationID);
                param.Add("@p_EmployeeGradeID", AddEmployeeWorker.EmployeeGradeID);
                param.Add("@p_EmployeeLevelID", AddEmployeeWorker.EmployeeLevelID);
                param.Add("@p_ContractorID", AddEmployeeWorker.ContractorID);
                param.Add("@p_JoiningDate", AddEmployeeWorker.JoiningDate);
                param.Add("@p_IsCriticalStageApplicable", AddEmployeeWorker.IsCriticalStageApplicable);
                param.Add("@p_EmployeeCriticalStageID", AddEmployeeWorker.EmployeeCriticalStageID);
                param.Add("@p_EmployeeLineID", AddEmployeeWorker.EmployeeLineID);
                param.Add("@p_EmployeeUnitID", AddEmployeeWorker.EmployeeUnitID);
                param.Add("@p_EmployeeAllocationCategoryId", AddEmployeeWorker.EmployeeAllocationCategoryId);


                //---------------------------------End Admin Info--------------------------------

                //param.Add("@p_AttendanceId", AddEmployeeWorker.AttendanceId);
                param.Add("@p_AttendanceId_Encrypted", AddEmployeeWorker.AttendanceId_Encrypted);
                param.Add("@p_AttendanceEmployeeId", AddEmployeeWorker.AttendanceEmployeeId);
                param.Add("@p_EM_Atten_WOFF1", AddEmployeeWorker.EM_Atten_WOFF1);
                param.Add("@p_EM_Atten_OT_Applicable", "1");
                param.Add("@p_EM_Atten_OTMultiplyBy", "1");
                param.Add("@p_EM_Atten_PerDayShiftHrs", "8.5");
                param.Add("@p_EM_Atten_ShiftGroupId", AddEmployeeWorker.EM_Atten_ShiftGroupId);
                param.Add("@p_EM_Atten_ShiftRuleId", AddEmployeeWorker.EM_Atten_ShiftRuleId);
                param.Add("@p_PHApplicable", AddEmployeeWorker.PHApplicable);

                //---------------------------------End attendance Info--------------------------------
                param.Add("@p_PersonalEmployeeId", AddEmployeeWorker.PersonalEmployeeId);
                param.Add("@p_PersonalId_Encrypted", AddEmployeeWorker.PersonalId_Encrypted);
                param.Add("@p_AadhaarNo", AddEmployeeWorker.AadhaarNo);
                param.Add("@p_PrimaryMobile", AddEmployeeWorker.PrimaryMobile);
                param.Add("@p_BirthdayDate", AddEmployeeWorker.BirthdayDate);
                param.Add("@p_EmployeeQualificationID", AddEmployeeWorker.EmployeeQualificationID);
                param.Add("@p_MaritalStatus", AddEmployeeWorker.MaritalStatus);
                param.Add("@p_Gender", AddEmployeeWorker.Gender);
                param.Add("@p_NameAsPerAadhaar", AddEmployeeWorker.NameAsPerAadhaar);
                param.Add("@p_EM_Atten_CanteenApplicable", AddEmployeeWorker.EM_Atten_CanteenApplicable);
                //---------------------------------End Personal Info--------------------------------

                //param.Add("@p_AddressId", AddEmployeeWorker.AddressId);
                param.Add("@p_AddressId_Encrypted", AddEmployeeWorker.AddressId_Encrypted);
                param.Add("@p_AddressEmployeeId", AddEmployeeWorker.AddressEmployeeId);
                param.Add("@p_PresentPin", AddEmployeeWorker.PresentPin);
                param.Add("@p_PresentState", AddEmployeeWorker.PresentState);
                param.Add("@p_PresentDistrict", AddEmployeeWorker.PresentDistrict);
                param.Add("@p_PresentTaluka", AddEmployeeWorker.PresentTaluka);
                param.Add("@p_PresentPO", AddEmployeeWorker.PresentPO);
                param.Add("@p_PresentCity", AddEmployeeWorker.PresentCity);
                param.Add("@p_PresentPostelAddress", AddEmployeeWorker.PresentPostelAddress);

                //---------------------------------End Address Info--------------------------------



                param.Add("@p_PF_FSType", AddEmployeeWorker.PF_FSType);
                param.Add("@p_PF_FS_Name", AddEmployeeWorker.PF_FS_Name);
                if (AddEmployeeWorker.PF_UAN == null)
                {
                    param.Add("@p_PF_UAN", "Pending");
                }
                else
                {
                    param.Add("@p_PF_UAN", AddEmployeeWorker.PF_UAN);
                }

                param.Add("@p_ESIC_NO", AddEmployeeWorker.ESIC_NO);


                //---------------------------------End statutory Info--------------------------------
                //---------------------------------Start IsAttendacneRegularized--------------------------------
                param.Add("@p_IsAttendacneRegularized", AddEmployeeWorker.IsAttendacnce);

                //---------------------------------End IsAttendacneRegularized--------------------------------
                // ---------------------------------Start CTC--------------------------------
                param.Add("@p_EM_Atten_DailyMonthly", AddEmployeeWorker.EM_Atten_DailyMonthly);
                //---------------------------------End CTC--------------------------------


                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                DapperORM.ExecuteReturn("sp_SUD_Mas_EmployeeWorkers", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["PId"] = param.Get<string>("@p_Id");
                return RedirectToAction("Module_Employee_AddWorkers", "Module_Employee_AddWorkers");

                //else
                //{
                //    param.Add("@p_process", "Rejoin");
                //    param.Add("@p_EmployeeId", AddEmployeeWorker.EmployeeId);
                //    param.Add("@p_EmployeeId_Encrypted", AddEmployeeWorker.EmployeeId_Encrypted);
                //    param.Add("@p_CmpID", AddEmployeeWorker.CmpID);
                //    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //    param.Add("@p_EmployeeBranchId", AddEmployeeWorker.EmployeeBranchId);
                //    param.Add("@p_EmployeeNo", AddEmployeeWorker.EmployeeNo);
                //    param.Add("@p_EmployeeCardNo", AddEmployeeWorker.EmployeeCardNo);
                //    param.Add("@p_Salutation", AddEmployeeWorker.Salutation);
                //    param.Add("@p_EmployeeName", AddEmployeeWorker.EmployeeName);
                //    param.Add("@p_EmployeeDepartmentID", AddEmployeeWorker.EmployeeDepartmentID);
                //    param.Add("@p_EmployeeSubDepartmentName", AddEmployeeWorker.EmployeeSubDepartmentName);
                //    param.Add("@p_EmployeeDesignationID", AddEmployeeWorker.EmployeeDesignationID);
                //    param.Add("@p_EmployeeGradeID", AddEmployeeWorker.EmployeeGradeID);
                //    param.Add("@p_EmployeeLevelID", AddEmployeeWorker.EmployeeLevelID);
                //    param.Add("@p_ContractorID", AddEmployeeWorker.ContractorID);
                //    param.Add("@p_JoiningDate", AddEmployeeWorker.JoiningDate);
                //    param.Add("@p_IsCriticalStageApplicable", AddEmployeeWorker.IsCriticalStageApplicable);
                //    param.Add("@p_EmployeeCriticalStageID", AddEmployeeWorker.EmployeeCriticalStageID);
                //    param.Add("@p_EmployeeLineID", AddEmployeeWorker.EmployeeLineID);
                //    param.Add("@p_EmployeeUnitID", AddEmployeeWorker.EmployeeUnitID);


                //    //---------------------------------End Admin Info--------------------------------

                //    //param.Add("@p_AttendanceId", AddEmployeeWorker.AttendanceId);
                //    param.Add("@p_AttendanceId_Encrypted", AddEmployeeWorker.AttendanceId_Encrypted);
                //    param.Add("@p_AttendanceEmployeeId", AddEmployeeWorker.AttendanceEmployeeId);
                //    param.Add("@p_EM_Atten_WOFF1", AddEmployeeWorker.EM_Atten_WOFF1);
                //    param.Add("@p_EM_Atten_OT_Applicable", AddEmployeeWorker.EM_Atten_OT_Applicable);
                //    param.Add("@p_EM_Atten_OTMultiplyBy", AddEmployeeWorker.EM_Atten_OTMultiplyBy);
                //    param.Add("@p_EM_Atten_PerDayShiftHrs", AddEmployeeWorker.EM_Atten_PerDayShiftHrs);
                //    param.Add("@p_EM_Atten_ShiftGroupId", AddEmployeeWorker.EM_Atten_ShiftGroupId);
                //    param.Add("@p_EM_Atten_ShiftRuleId", AddEmployeeWorker.EM_Atten_ShiftRuleId);

                //    //---------------------------------End attendance Info--------------------------------
                //    param.Add("@p_PersonalEmployeeId", AddEmployeeWorker.PersonalEmployeeId);
                //    param.Add("@p_PersonalId_Encrypted", AddEmployeeWorker.PersonalId_Encrypted);
                //    param.Add("@p_AadhaarNo", AddEmployeeWorker.AadhaarNo);
                //    param.Add("@p_PrimaryMobile", AddEmployeeWorker.PrimaryMobile);
                //    param.Add("@p_BirthdayDate", AddEmployeeWorker.BirthdayDate);
                //    param.Add("@p_EmployeeQualificationID", AddEmployeeWorker.EmployeeQualificationID);
                //    param.Add("@p_MaritalStatus", AddEmployeeWorker.MaritalStatus);
                //    param.Add("@p_Gender", AddEmployeeWorker.Gender);

                //    //---------------------------------End Personal Info--------------------------------

                //    //param.Add("@p_AddressId", AddEmployeeWorker.AddressId);
                //    param.Add("@p_AddressId_Encrypted", AddEmployeeWorker.AddressId_Encrypted);
                //    param.Add("@p_AddressEmployeeId", AddEmployeeWorker.AddressEmployeeId);
                //    param.Add("@p_PresentPin", AddEmployeeWorker.PresentPin);
                //    param.Add("@p_PresentState", AddEmployeeWorker.PresentState);
                //    param.Add("@p_PresentDistrict", AddEmployeeWorker.PresentDistrict);
                //    param.Add("@p_PresentTaluka", AddEmployeeWorker.PresentTaluka);
                //    param.Add("@p_PresentPO", AddEmployeeWorker.PresentPO);
                //    param.Add("@p_PresentCity", AddEmployeeWorker.PresentCity);
                //    param.Add("@p_PresentPostelAddress", AddEmployeeWorker.PresentPostelAddress);

                //    //---------------------------------End Address Info--------------------------------
                //    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                //    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    DapperORM.ExecuteReturn("sp_SUD_Mas_EmployeeWorkers", param);
                //    TempData["Message"] = param.Get<string>("@p_msg");
                //    TempData["Icon"] = param.Get<string>("@p_Icon");
                //    TempData["PId"] = param.Get<string>("@p_Id");
                //    return RedirectToAction("Module_Employee_AddWorkers", "Module_Employee_AddWorkers");
                //}
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetPincode
        [HttpGet]
        public ActionResult GetPincode(int PinCode)
        {
            try
            {  //PinCode
                param.Add("@p_PinCode", PinCode);
                var GetPinCodelist = DapperORM.ExecuteSP<dynamic>("[sp_List_Mas_Employee_PinCode]", param).ToList();
                return Json(new { data = GetPinCodelist }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetIFSCCode
        [HttpGet]
        public ActionResult GetIFSCCode(string IFSCode)
        {
            try
            {
                param.Add("@p_IFSCode", IFSCode);
                var GetIFSCCodelist = DapperORM.ExecuteSP<dynamic>("[sp_List_Mas_Employee_IFSCCode]", param).ToList();
                return Json(new { data = GetIFSCCodelist }, JsonRequestBehavior.AllowGet);
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

        #region ActiveWorkerList
        public ActionResult ActiveWorkerList(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 323;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_EmployeeId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_ActiveWorker", param).ToList();
                ViewBag.GetActiveWorkerList = data;

                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "SubForm", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "SubForm", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }

                Session["WorkerOnboardEmployeeId"] = null;
                Session["WorkerOnboardCmpId"] = null;
                Session["WorkerOnboardBranchId"] = null;
                Session["WorkerOnboardEmployeeName"] = null;
                Session["WorkerOnboardEmpDesignation"] = null;
                Session["WorkerOnboardEmployeeNo"] = null;
                Session["WorkerOnboardEmpDepartment"] = null;
                Session["WorkerOnboardEmpDoj"] = null;
                Session["WorkerOnboardEmpDepartmentId"] = null;
                Session["WorkerFirstName"] = null;
                Session["WorkerContractorName"] = null;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployeeHistory
        public ActionResult GetEmployeeHistory(string AddharNo)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramy = new DynamicParameters();
                paramy.Add("@query", "Select * from View_EmployeeHistory where AadhaarNo='" + AddharNo + "'");
                var GetEmployeeHistory = DapperORM.ExecuteSP<EmployeeHistory>("sp_QueryExcution", paramy).ToList();
                return Json(GetEmployeeHistory, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region OnboardingForm
        public ActionResult WorkerOnboardingForm(int EmpId, string EmpName, string EmpDesignation, string EmployeeNo, string EmpDepartment, string EmpDoj, string CmpId, string BranchId, string EmpDepartmentId, string FirstName, string ContractorName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                Session["WorkerOnboardEmployeeId"] = EmpId;
                Session["WorkerOnboardCmpId"] = CmpId;
                Session["WorkerOnboardBranchId"] = BranchId;
                Session["WorkerOnboardEmployeeName"] = EmpName;
                Session["WorkerOnboardEmpDesignation"] = EmpDesignation;
                Session["WorkerOnboardEmployeeNo"] = EmployeeNo;
                Session["WorkerOnboardEmpDepartment"] = EmpDepartment;
                Session["WorkerOnboardEmpDoj"] = EmpDoj;
                Session["WorkerOnboardEmpDepartmentId"] = EmpDepartmentId;
                Session["WorkerFirstName"] = FirstName;
                Session["WorkerContractorName"] = ContractorName;

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", EmpId);
                var SetupFlag = DapperORM.DynamicList("sp_List_Mas_Employee_StatusCheck", paramList);
                return Json(SetupFlag, JsonRequestBehavior.AllowGet);
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


    }
}