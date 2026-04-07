using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_OrganisationChart_MappingController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: Module/Module_Employee_OrganisationChart_Mapping
        public ActionResult Module_Employee_OrganisationChart_Mapping(long? TypeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 540;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select OrganisationTypeId as Id , OrganisationTypeName Name from Organisation_Type where Deactivate=0");
                ViewBag.OrganisationType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                //var SetTypeId = Convert.ToInt64(ViewBag.OrganisationType[0].Id);
                //var SendTypeId="";
                //if (TypeId != null)
                //{
                //    SendTypeId = TypeId;
                //    ViewBag.SelectedTypeId = TypeId;
                //}
                //else
                //{
                //    SendTypeId = SetTypeId;
                //    ViewBag.SelectedTypeId = SetTypeId;
                //}

                //long? SendTypeId = null;
                //long? SetTypeId = Convert.ToInt64(ViewBag.OrganisationType[0].Id);
                //SendTypeId = TypeId ?? SetTypeId;  // Simplified assignment
                //ViewBag.SelectedTypeId = SendTypeId;

                var GetEmployee = new BulkAccessClass().AllEmployeeName();
                ViewBag.AllEmployeeName = GetEmployee;


                if (TypeId != null)
                {
                    var GetOrgList = DapperORM.DynamicQueryList($@"SELECT 
                        Organisation_Chart.OrganisationChartId, 
                        Organisation_Chart.OrganisationTypeId, 
                        Organisation_Level.OrganisationLevel, 
                        Organisation_Chart.OrganisationSubLevelId, 
                        Organisation_Chart.OrganisationTitle,
                        Organisation_Chart.OrganisationRemark, 
                        Organisation_Chart.OrganisationEmployeeId 
                    FROM Organisation_Chart 
                    JOIN Organisation_Level 
                        ON Organisation_Chart.OrganisationLevelId = Organisation_Level.OrganisationLevelId
                    WHERE Organisation_Chart.Deactivate = 0  
                      AND Organisation_Level.Deactivate = 0 
                      AND Organisation_Chart.OrganisationTypeId = {TypeId}
                      AND CAST(SUBSTRING(Organisation_Level.OrganisationLevel, 2, LEN(Organisation_Level.OrganisationLevel)) AS INT) IS NOT NULL").ToList();

                    ViewBag.OrganisationList = GetOrgList;


                    ViewBag.SelectedTypeId = TypeId;
                }
                else
                {
                    ViewBag.OrganisationList = null;
                }
                
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        public class RequestModel
        {
            public string OrganisationLevel { get; set; }
            public string OrganisationTitle { get; set; }
            public string OrganisationRemark { get; set; }
            public int EmployeeId { get; set; }
            public int OrgChartId { get; set; }
        }


        [HttpPost]
        public JsonResult SaveRequest(List<RequestModel> RequestArray)
        {
            try
            {
                if (RequestArray != null && RequestArray.Count > 0)
                {
                    //using (var connection = new SqlConnection(DapperORM.connectionString))
                    //{
                    //    connection.Open();

                    //    foreach (var item in RequestArray)
                    //    {
                    //        string query = $@"
                    //        UPDATE Organisation_Chart 
                    //        SET OrganisationEmployeeId = {item.EmployeeId} 
                    //        WHERE OrganisationChartId = {item.OrgChartId}";
                    //        connection.Execute(query, item);
                    //    }
                    //}
                    foreach(var item in RequestArray)
                    {
                        DynamicParameters paramid = new DynamicParameters();
                        paramid.Add("@query", "UPDATE Organisation_Chart SET OrganisationEmployeeId = '" + item.EmployeeId + "' WHERE OrganisationChartId = '" + item.OrgChartId + "'");
                        var OrgChart = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramid).FirstOrDefault();
                    }

                    TempData["Message"] = "Record save successfully";
                    TempData["Icon"] = "success";
                    return Json(new { success = true, message = TempData["Message"], icon = TempData["Icon"] });
                }
                else
                {
                    return Json(new { success = false, message = "No data received" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


    }
}