using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Contractor;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Contractor
{
    public class Setting_Contractor_BulkUpdateTypeMappingController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_Contractor_BulkUpdateTypeMapping
        #region Setting_Contractor_BulkUpdateTypeMapping
        public ActionResult Setting_Contractor_BulkUpdateTypeMapping()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 477;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                ViewBag.AddUpdateTitle = "Add";
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorId=1 order by Name");
                var EmpolyeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmpolyeeName = EmpolyeeName;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "Setting_Contractor_BulkUpdateTypeMapping");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 124;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_TypeMappingId_Encrypted", "List");
                var data = DapperORM.ReturnList<dynamic>("sp_List_Mas_BulkUpdateTypeMapping", param).ToList();
                ViewBag.getEmployeeName = data;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpadte
        [HttpPost]
        public ActionResult SaveUpdate(List<Mas_BulkUpdateTypeMapping> BulkUpdateTypeMapping)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (BulkUpdateTypeMapping.Count > 0)
                {

                    for (var i = 0; i < BulkUpdateTypeMapping.Count; i++)
                    {
                        param.Add("@p_process", "Save");
                        param.Add("@p_TypeId", BulkUpdateTypeMapping[i].TypeId);
                        param.Add("@p_EmployeeId", BulkUpdateTypeMapping[i].EmployeeId);
                        param.Add("@p_IsActive", BulkUpdateTypeMapping[i].IsActive);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_Mas_BulkUpdateTypeMapping", param);
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

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBulkType(int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select isnull(Mas_BulkUpdateTypeMapping.IsActive,0) as IsActive,Mas_BulkUpdateType.BulkUpdateTypeId,Mas_BulkUpdateType.TypeName from Mas_BulkUpdateType left JOIN  Mas_BulkUpdateTypeMapping on Mas_BulkUpdateTypeMapping.TypeId = Mas_BulkUpdateType.BulkUpdateTypeId and Mas_BulkUpdateTypeMapping.EmployeeId=" + EmployeeId + " where Mas_BulkUpdateType.Deactivate = 0");
                //= DapperORM.DynamicQuerySingle(TypeName);
                var GetTypeName = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();
                return Json(GetTypeName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Delete
        public ActionResult Delete(int? EmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EmployeeId", EmployeeID);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_BulkUpdateTypeMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Contractor_BulkUpdateTypeMapping");
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