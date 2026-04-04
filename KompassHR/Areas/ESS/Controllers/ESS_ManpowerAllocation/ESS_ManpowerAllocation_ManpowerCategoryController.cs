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
    public class ESS_ManpowerAllocation_ManpowerCategoryController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_ManpowerAllocation_ManpowerCategory
        #region ManpowerCategory Main View 
        public ActionResult ESS_ManpowerAllocation_ManpowerCategory(string KPISubCategoryId_Encrypted,int? KPISubCategoryId,int? CmpID, KPI_SubCategory _SubCategory)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 279;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramList = new DynamicParameters();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;
                //DynamicParameters paramCategory = new DynamicParameters();
                //paramCategory.Add("@query", "   Select KPICategoryId as Id ,KPICategoryName as Name from KPI_Category where Deactivate=0 order by Name");
                //var GetKPICategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory).ToList();
                //ViewBag.KPICategoryName = GetKPICategoryName;

                if(_SubCategory.KPISubCategoryBranchId!=0 && _SubCategory.CmpID != 0)
                {
                    DynamicParameters Designation = new DynamicParameters();
                    Designation.Add("@query", @"SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0 and DesignationId NOT in (select  KPI_CategoryDepartment.KPI_Category_DesignationId from KPI_CategoryDepartment ,KPI_SubCategory where KPI_SubCategory.Deactivate=0 and KPI_SubCategory.Deactivate=0 and KPI_CategoryDepartment.KPI_Category_CategoryId=KPI_SubCategory.KPISubCategoryId and KPI_SubCategory.KPISubCategoryBranchId=" + _SubCategory.KPISubCategoryBranchId + ")order by Name;");
                    ViewBag.GetDesignationName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", Designation).ToList();

                    //var results = DapperORM.DynamicQuerySingle(@"SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0 and DesignationId NOT in (select  KPI_CategoryDepartment.KPI_Category_DesignationId from KPI_CategoryDepartment ,KPI_SubCategory where KPI_SubCategory.Deactivate=0 and KPI_SubCategory.Deactivate=0 and KPI_CategoryDepartment.KPI_Category_CategoryId=KPI_SubCategory.KPISubCategoryId and KPI_SubCategory.KPISubCategoryBranchId="+ _SubCategory.KPISubCategoryBranchId + ")order by Name;");
                    //ViewBag.GetDesignationName = results.Read<AllDropDownClass>().ToList();

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", _SubCategory.CmpID);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.GetBranchName = data;
                    TempData["KPISubCategoryBranchId"] = "";
                    TempData["KPISubCategoryFName"] = "";
                    if (KPISubCategoryId_Encrypted != "")
                    {
                        //var resultss = DapperORM.DynamicQuerySingleMultiple(@"SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0 order by DesignationId;
                        //                             select CompanyId as Id ,CompanyName as Name from Mas_CompanyProfile where Deactivate = 0;");
                        //ViewBag.GetDesignationName = resultss.Read<AllDropDownClass>().ToList();
                        //ViewBag.GetCompanyName = resultss.Read<AllDropDownClass>().ToList();

                        //ViewBag.AddUpdateTitle = "Update";
                        //DynamicParameters ShiftGroup = new DynamicParameters();
                        //ShiftGroup.Add("@p_KPISubCategoryId_Encrypted", KPISubCategoryId_Encrypted);
                        //KPICategory = DapperORM.ReturnList<KPI_SubCategory>("sp_List_KPI_SubCategory", ShiftGroup).FirstOrDefault();
                        //var KPIBranchId = KPICategory.KPISubCategoryBranchId;

                        //DynamicParameters paramBranch1 = new DynamicParameters();
                        //paramBranch1.Add("@p_employeeid", Session["EmployeeId"]);
                        //paramBranch1.Add("@p_CmpId", CmpID);
                        //var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch1).ToList();
                        //ViewBag.GetBranchName = data1;

                        //DynamicParameters param1 = new DynamicParameters();
                        //param1.Add("@query", @"Select '1' as CheckBox, KPI_Category_DesignationId  from  KPI_CategoryDepartment where KPI_Category_CategoryId =" + KPISubCategoryId + " union  Select '0' as CheckBox,Mas_Designation.DesignationId  from  Mas_Designation where  Deactivate=0 and DesignationId not in (select  KPI_CategoryDepartment.KPI_Category_DesignationId from KPI_CategoryDepartment where KPI_Category_CategoryId=" + KPISubCategoryId + ")order by KPI_CategoryDepartment.KPI_Category_DesignationId");
                        //ViewBag.SelectedManPowerList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param1).ToList();
                    }
                }
                else
                {
                    ViewBag.GetBranchName = "";
                    ViewBag.GetDesignationName = "";
                    if (KPISubCategoryId_Encrypted != null)
                    {
                        KPI_SubCategory KPICategory = new KPI_SubCategory();
                        var resultss = DapperORM.DynamicQueryMultiple(@"SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0 order by DesignationId;
                                                     select CompanyId as Id ,CompanyName as Name from Mas_CompanyProfile where Deactivate = 0;");
                        ViewBag.GetDesignationName = resultss[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                        ViewBag.GetCompanyName = resultss[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                        ViewBag.AddUpdateTitle = "Update";
                        DynamicParameters ShiftGroup = new DynamicParameters();
                        ShiftGroup.Add("@p_KPISubCategoryId_Encrypted", KPISubCategoryId_Encrypted);
                        KPICategory = DapperORM.ReturnList<KPI_SubCategory>("sp_List_KPI_SubCategory", ShiftGroup).FirstOrDefault();
                        var KPIBranchId = KPICategory.KPISubCategoryBranchId;
                        TempData["KPISubCategoryBranchId"] = KPICategory.KPISubCategoryBranchId;
                        TempData["KPISubCategoryFName"] = KPICategory.KPISubCategoryFName;

                        DynamicParameters paramBranch1 = new DynamicParameters();
                        paramBranch1.Add("@p_employeeid", Session["EmployeeId"]);
                        paramBranch1.Add("@p_CmpId", CmpID);
                        var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch1).ToList();
                        ViewBag.GetBranchName = data1;

                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@query", @"Select '1' as CheckBox, KPI_Category_DesignationId  from  KPI_CategoryDepartment where KPI_Category_CategoryId =" + KPISubCategoryId + " union  Select '0' as CheckBox,Mas_Designation.DesignationId  from  Mas_Designation where  Deactivate=0 and DesignationId not in (select  KPI_CategoryDepartment.KPI_Category_DesignationId from KPI_CategoryDepartment where KPI_Category_CategoryId=" + KPISubCategoryId + ")order by KPI_CategoryDepartment.KPI_Category_DesignationId");
                        ViewBag.SelectedManPowerList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param1).ToList();
                        return View(KPICategory);
                    }
                }

                //if(KPISubCategoryId_Encrypted!=null || KPISubCategoryId_Encrypted != "")
                //{
                //    var resultss = DapperORM.DynamicQuerySingleMultiple(@"SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0 order by DesignationId;
                //                                     select CompanyId as Id ,CompanyName as Name from Mas_CompanyProfile where Deactivate = 0;");
                //    ViewBag.GetDesignationName = resultss.Read<AllDropDownClass>().ToList();
                //    ViewBag.GetCompanyName = resultss.Read<AllDropDownClass>().ToList();

                //    ViewBag.AddUpdateTitle = "Update";
                //    DynamicParameters ShiftGroup = new DynamicParameters();
                //    ShiftGroup.Add("@p_KPISubCategoryId_Encrypted", KPISubCategoryId_Encrypted);
                //    KPICategory = DapperORM.ReturnList<KPI_SubCategory>("sp_List_KPI_SubCategory", ShiftGroup).FirstOrDefault();
                //    var KPIBranchId = KPICategory.KPISubCategoryBranchId;

                //    DynamicParameters paramBranch = new DynamicParameters();
                //    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                //    paramBranch.Add("@p_CmpId", CmpID);
                //    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                //    ViewBag.GetBranchName = data;

                //    DynamicParameters param1 = new DynamicParameters();
                //    param1.Add("@query", @"Select '1' as CheckBox, KPI_Category_DesignationId  from  KPI_CategoryDepartment where KPI_Category_CategoryId =" + KPISubCategoryId + " union  Select '0' as CheckBox,Mas_Designation.DesignationId  from  Mas_Designation where  Deactivate=0 and DesignationId not in (select  KPI_CategoryDepartment.KPI_Category_DesignationId from KPI_CategoryDepartment where KPI_Category_CategoryId=" + KPISubCategoryId + ")order by KPI_CategoryDepartment.KPI_Category_DesignationId");
                //    ViewBag.SelectedManPowerList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param1).ToList();
                //}
                //else
                //{
                //    ViewBag.GetBranchName = "";
                //    var results = DapperORM.DynamicQuerySingleMultiple(@"SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0 and DesignationId  NOT in (select  KPI_CategoryDepartment.KPI_Category_DesignationId from KPI_CategoryDepartment)order by DesignationId;");
                //    ViewBag.GetDesignationName = results.Read<AllDropDownClass>().ToList();
                //}
                return View(_SubCategory);

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
        [HttpPost]
        public ActionResult IsManpowerAllocationExists(string KPISubCategoryId_Encrypted, int? CompanyName, int? KPICategoryBranchId, string KPISubCategoryFName, List<KPI_DepartmentId> lstKPICategory)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }            
                string result = "";
                for (var i = 0; i < lstKPICategory.Count; i++)
                {
                    result = result + lstKPICategory[i].KPIDepartmentId + ",";                  
                }

                result = result.TrimEnd(',');
                Session["lstKPICategory"] = result;
                param.Add("@p_process", "IsValidation");
                param.Add("@p_KPISubCategoryId_Encrypted", KPISubCategoryId_Encrypted);
                param.Add("@p_KPISubCategoryBranchId", KPICategoryBranchId);
                param.Add("@p_CmpID", CompanyName);
                param.Add("@p_KPISubCategoryFName", KPISubCategoryFName);
                param.Add("@P_KPI_SubCategory_DesignationIds", result);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_KPI_SubCategory", param);
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
        public ActionResult SaveUpdate(KPI_SubCategory ObjKPICategory)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var lstKPICategory = Session["lstKPICategory"];

                param.Add("@p_process", string.IsNullOrEmpty(ObjKPICategory.KPISubCategoryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_KPISubCategoryId_Encrypted", ObjKPICategory.KPISubCategoryId_Encrypted);
                param.Add("@p_CmpID", ObjKPICategory.CmpID);
                param.Add("@p_KPISubCategoryBranchId", ObjKPICategory.KPISubCategoryBranchId);
                param.Add("@p_KPISubCategoryFName", ObjKPICategory.KPISubCategoryFName);
                //param.Add("@p_KPISubCategorySName", ObjKPICategory.KPISubCategorySName);
                param.Add("@P_KPI_SubCategory_DesignationIds", lstKPICategory);              
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_KPI_SubCategory", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("ESS_ManpowerAllocation_ManpowerCategory", "ESS_ManpowerAllocation_ManpowerCategory");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 279;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_KPISubCategoryId_Encrypted", "List");
                param.Add("@P_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.DynamicList("sp_List_KPI_SubCategory", param);
                ViewBag.GetKPICategoryList = data;
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
        public ActionResult Delete(double? KPISubCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_KPISubCategoryId", KPISubCategoryId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_KPI_SubCategory", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_ManpowerAllocation_ManpowerCategory");
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