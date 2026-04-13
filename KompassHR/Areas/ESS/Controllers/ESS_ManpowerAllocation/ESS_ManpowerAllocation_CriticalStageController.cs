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
    public class ESS_ManpowerAllocation_CriticalStageController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_ManpowerAllocation_CriticalStage
        #region CriticalStage
        public ActionResult ESS_ManpowerAllocation_CriticalStage(KPI_CriticalStage KPICriticalStage)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 291;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var results = DapperORM.DynamicQueryMultiple(@"select CompanyId as Id ,CompanyName as Name from Mas_CompanyProfile where Deactivate = 0;
                                                     select DepartmentId as Id ,DepartmentName as Name from Mas_Department where Deactivate = 0
                                                     select GradeId as Id ,GradeName as Name from Mas_Grade where Deactivate = 0
                                                     select CriticalStageId as Id ,CriticalStageName as Name from Mas_CriticalStageCategory where Deactivate = 0");
                ViewBag.GetCompanyName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetDepatmentList = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetGradeList = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetCriticalCategoryList = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                //ViewBag.GetCompanyName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetDepatmentList = results.Read<AllDropDownClass>().ToList();              
                //ViewBag.GetGradeList = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetCriticalCategoryList = results.Read<AllDropDownClass>().ToList();
                ViewBag.GetBranchName = "";

                if(KPICriticalStage.CmpID!=0)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", "and  EmployeeId not in (select CriticalStageEmployeeId from KPI_CriticalStage where Deactivate=0) and Mas_Employee.EmployeeBranchId= "+ KPICriticalStage .KPICriticalStageBranchId+ " and Mas_Employee.EmployeeDepartmentID=" + KPICriticalStage.CriticalStageDepartmentId + " and Mas_Employee.EmployeeGradeID=" + KPICriticalStage.CriticalStageGradeId + "order by EmployeeCardNo ");
                    ViewBag.GetEmployeeList = DapperORM.ExecuteSP<dynamic>("sp_GetEmployeeList", paramList).ToList();

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", KPICriticalStage.CmpID);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.GetBranchName = data;
                }
                //else if (KPICriticalStage.CmpID != 0 && ViewBag.GetEmployeeList.Count!=0) {
                //    ViewBag.NullGetEmployeeList = "";
                    
                //}
                else
                {
                    ViewBag.GetEmployeeList = "";
                    ViewBag.NullGetEmployeeList = "Record Not Found";
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(List<KPIBulkRecord> TaskKPI,int CmpId,int KPICriticalStageBranchId,string CriticalStageFromDate,string CriticalStageToDate,string CriticalStageGradeId,string CriticalStageDepartmentId,string CriticalStageCategoryId)
        {
            try
            {
                if (TaskKPI != null)
                {
                    for (var i = 0; i < TaskKPI.Count; i++)
                    {
                        param.Add("@p_process", "Save");                    
                        param.Add("@p_CmpID",CmpId);
                        param.Add("@p_KPICriticalStageBranchId",KPICriticalStageBranchId);
                        param.Add("@p_CriticalStageFromDate", CriticalStageFromDate);
                        param.Add("@p_CriticalStageToDate", CriticalStageToDate);
                        param.Add("@p_CriticalStageGradeId", CriticalStageGradeId);
                        param.Add("@p_CriticalStageDepartmentId", CriticalStageDepartmentId);
                        param.Add("@p_CriticalStageCategoryId", CriticalStageCategoryId);
                        param.Add("@p_CriticalStageEmployeeId", TaskKPI[i].CriticalStageEmployeeId);
                        param.Add("@p_CriticalStageShiftGroupId", TaskKPI[i].CriticalStageShiftGroupId);
                        param.Add("@p_CriticalStageShiftRuleId", TaskKPI[i].CriticalStageShiftRuleId);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_KPI_CriticalStage", param);
                        TempData["Message"] = param.Get<string>("@p_msg");
                        TempData["Icon"] = param.Get<string>("@p_Icon");
                    }
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return RedirectToAction("ESS_ManpowerAllocation_CriticalStage", "ESS_ManpowerAllocation_CriticalStage");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList(KPI_CriticalStage KPICriticalStageList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 291;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var results = DapperORM.DynamicQueryMultiple(@"select CompanyId as Id ,CompanyName as Name from Mas_CompanyProfile where Deactivate = 0;
                                                     select DepartmentId as Id ,DepartmentName as Name from Mas_Department where Deactivate = 0 ");
                ViewBag.GetCompanyName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetDepatmentList = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                //ViewBag.GetCompanyName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetDepatmentList = results.Read<AllDropDownClass>().ToList();

                if (KPICriticalStageList.CmpID!=0)
                {
                    DynamicParameters PList= new DynamicParameters();
                    PList.Add("@p_KPICriticalStageBranchId", KPICriticalStageList.KPICriticalStageBranchId);
                    PList.Add("@p_CriticalStageDepartmentId", KPICriticalStageList.CriticalStageDepartmentId);
                    PList.Add("@p_Month", KPICriticalStageList.CriticalStageFromDate);
                    var GetCriticalList = DapperORM.DynamicList("sp_List_KPI_CriticalStage", PList);
                    ViewBag.CriticalStageList = GetCriticalList;

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", KPICriticalStageList.CmpID);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.GetBranchName = data;
                }
                else
                {
                    ViewBag.GetBranchName = "";
                    ViewBag.CriticalStageList = "";
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
        public ActionResult Delete(string KPICriticalStageId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_KPICriticalStageId_Encrypted", KPICriticalStageId_Encrypted);               
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_KPI_CriticalStage", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_ManpowerAllocation_CriticalStage");
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