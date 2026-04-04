using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ManpowerAllocation
{
    public class ESS_Employee_Line_ManpowerCategoryMappingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        // GET: Setting/ ESS_Employee_Line_ManpowerCategoryMapping
        #region Main View
        public ActionResult ESS_Employee_Line_ManpowerCategoryMapping(Manpower_LineManpowerCategoryMap Manpower_LineManpowerCategoryMap)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Check access permissions
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 771;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT LineId as Id, LineName as Name FROM Mas_LineMaster WHERE Deactivate = 0 ORDER BY Name");
                var Line = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetLine = Line;
                ViewBag.GetBranchName = "";
                ViewBag.GetUnitName = "";

                if (Manpower_LineManpowerCategoryMap.LineId > 0)
                {
                    param.Add("@query", $@"Select LineManpowerCategoryMapId,KPI_SubCategory.KPISubCategoryId ,IsNull(Manpower_LineManpowerCatMapping.IsActive,0)IsActive ,KPI_SubCategory.KPISubCategoryFName 
                          from KPI_SubCategory Left outer Join Manpower_LineManpowerCatMapping on Manpower_LineManpowerCatMapping.KPISubCategoryId=KPI_SubCategory.KPISubCategoryId 
						  and Manpower_LineManpowerCatMapping.LineId={Manpower_LineManpowerCategoryMap.LineId} and Manpower_LineManpowerCatMapping.CmpId={Manpower_LineManpowerCategoryMap.CmpId}  and
                        Manpower_LineManpowerCatMapping.BranchId= {Manpower_LineManpowerCategoryMap.BranchId} and Manpower_LineManpowerCatMapping.Deactivate=0 where KPI_SubCategory.Deactivate=0 order by KPISubCategoryFName ");
                    var data = DapperORM.ReturnList<Manpower_LineManpowerCategoryMap>("sp_QueryExcution", param).ToList();
                    ViewBag.GetManpowerCategoryList = data;

                    DynamicParameters paramBusinessUnit = new DynamicParameters();
                    paramBusinessUnit.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBusinessUnit.Add("@p_CmpId", Manpower_LineManpowerCategoryMap.CmpId);
                    var BusinessUnit = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBusinessUnit).ToList();
                    ViewBag.GetBranchName = BusinessUnit;
                }
                else
                {
                    ViewBag.GetManpowerCategoryList = "";
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

        #region GetSubUnit
        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetSubUnit(int BranchId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT UnitId as Id, UnitName as Name FROM Mas_Unit WHERE Deactivate = 0 and UnitBranchId='"+BranchId+"' ORDER BY Name");
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
        #endregion

        #region SaveUpadte
        [HttpPost]
        public ActionResult SaveUpadte(List<Manpower_LineManpowerCategoryMap> LineManpowerCategoryMap, string LineId, string CmpId, string BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 771;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
               
                if (LineManpowerCategoryMap.Count > 0)
                {
                    for (var i = 0; i < LineManpowerCategoryMap.Count; i++)
                    {
                        param.Add("@p_process", "Save");
                        param.Add("@p_LineId", LineManpowerCategoryMap[i].LineId);
                        param.Add("@p_IsActive", LineManpowerCategoryMap[i].IsActive);
                        param.Add("@p_KPISubCategoryId", LineManpowerCategoryMap[i].KPISubCategoryId);
                        param.Add("@p_CmpId", CmpId);
                        param.Add("@p_BranchId", BranchId);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_Manpower_LineManpowerCatMapping", param);
                        TempData["Message"] = param.Get<string>("@p_msg");
                        TempData["Icon"] = param.Get<string>("@p_Icon");

                    }
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
         
        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 771;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Origin", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Manpower_LineManpowerCatMapping", param);
                ViewBag.GetLineManpowerCatMapping = data;


                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ShowManpowerCategoryList
        [HttpGet]
        public ActionResult ShowManpowerCategoryList(int? CmpId, int? BranchId, int LineId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_CmpId", CmpId);
            param.Add("@p_BranchId", BranchId);
            param.Add("@p_LineId", LineId);
            var data = DapperORM.ReturnList<dynamic>("sp_List_Manpower_ManpowerCategoryList", param).ToList();
            string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete
        public ActionResult Delete(double? LineId, int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 771;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_LineId", LineId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Manpower_LineManpowerCatMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Employee_Line_ManpowerCategoryMapping");
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