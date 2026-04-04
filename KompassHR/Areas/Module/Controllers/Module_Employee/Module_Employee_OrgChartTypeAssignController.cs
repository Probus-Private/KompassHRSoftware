using Dapper;
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
    public class Module_Employee_OrgChartTypeAssignController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Employee_OrgChartTypeAssign
        public ActionResult Module_Employee_OrgChartTypeAssign()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 509;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                //param.Add("@query", $@"Select distinct Organisation_Type.OrganisationTypeId AS Id, Organisation_Type.OrganisationTypeName as Name from Organisation_Type,Organisation_Chart WHERE Organisation_Type.Deactivate=0 and Organisation_Type.ShowForEmployee=0
                //                    and Organisation_Type.OrganisationTypeId = Organisation_Chart.OrganisationTypeId and Organisation_Chart.Deactivate = 0
                //                    order by OrganisationTypeName");


                param.Add("@query", $@"select distinct Organisation_Chart.OrganisationTypeId as Id,Organisation_Type.OrganisationTypeName as Name from Organisation_Type 
                                     LEFT JOIN Organisation_Chart on Organisation_Chart.OrganisationTypeId=Organisation_Type.OrganisationTypeId
                                     WHERE Organisation_Type.Deactivate=0 AND  Organisation_Chart.Deactivate = 0 order by OrganisationTypeName");
                var GetTypeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetType = GetTypeList;

                param = new DynamicParameters();
                param.Add("@query", $@"SELECT MEO.OrgId,MEO.OrgTypeId_Encrypted, MEO.Deactivate,
                                        MEO.UseBy,MEO.CreatedBy,MEO.CreatedDate,MEO.ModifiedBy,
                                        MEO.ModifiedDate,MEO.MachineName,MEO.OrgEmployeeId,OT.OrganisationTypeName AS OrgTypeName
                                    FROM Mas_Employee_Organisation MEO
                                    LEFT JOIN Organisation_Type OT ON MEO.OrgTypeId = OT.OrganisationTypeId
                                    WHERE MEO.OrgEmployeeId = {Session["OnboardEmployeeId"]} 
                                    AND MEO.Deactivate = 0;");
                var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                ViewBag.GetAssetList = data;

                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region SaveUpdate
        [HttpGet]
        public ActionResult SaveUpdate(string OrgTypeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param = new DynamicParameters();
                param.Add("@p_OrgTypeId ", OrgTypeId);
                param.Add("@p_OrgEmployeeId ", Session["OnboardEmployeeId"]);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_OrganisationChart_TypeAssign", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("Module_Employee_OrgChartTypeAssign", "Module_Employee_OrgChartTypeAssign");
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
        public ActionResult Delete(string OrgTypeId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //string connectionString = Session["MyNewConnectionString"]?.ToString();
                //using (var connection = new SqlConnection(connectionString))
                //{
                //    string query = "DELETE FROM Mas_Employee_Organisation WHERE OrgTypeId_Encrypted = @OrgTypeId_Encrypted";
                //    connection.Execute(query, new { OrgTypeId_Encrypted = OrgTypeId_Encrypted });
                //}

                var query = @"DELETE FROM Mas_Employee_Organisation WHERE OrgTypeId_Encrypted = @OrgTypeId_Encrypted";
                DapperORM.Executes(query, new { OrgTypeId_Encrypted = OrgTypeId_Encrypted });

                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_OrgChartTypeAssign", "Module_Employee_OrgChartTypeAssign");
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