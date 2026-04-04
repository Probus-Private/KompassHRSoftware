using Dapper;
using KompassHR.Areas.Setting.Models.Setting_EmployeeGrievance;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers
{
    public class Setting_Employee_Grievance_CategoryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Employee_Grievance_Category
        #region Employee_Grievance_Category
        public ActionResult Setting_Employee_Grievance_Category(string GrievanceCategoryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "setting" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 672;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                Employee_Grievance_Category Grievance_Category = new Employee_Grievance_Category();
                if (GrievanceCategoryId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_GrievanceCategoryId_Encrypted", GrievanceCategoryId_Encrypted);
                    Grievance_Category = DapperORM.ReturnList<Employee_Grievance_Category>("sp_List_Employee_Grievance_Category", param).FirstOrDefault();
                }
                return View(Grievance_Category);
                
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

#endregion

        #region GetList Main View 
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "setting" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 672;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_GrievanceCategoryId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Employee_Grievance_Category", param).ToList();
                ViewBag.GetGrievanceCategoryList = data;
                
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsGrievanceCategoryExists
        public ActionResult IsGrievanceCategoryExists( string GrievanceCategory, string GrievanceCategoryId_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                   // param.Add("@p_GrievanceCategoryId", GrievanceCategoryId);
                    param.Add("@p_GrievanceCategory", GrievanceCategory);
                    param.Add("@p_GrievanceCategoryId_Encrypted", GrievanceCategoryId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Employee_Grievance_Category", param);
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
        public ActionResult SaveUpdate(Employee_Grievance_Category Grievance_Category)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "Setting" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(Grievance_Category.GrievanceCategoryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_GrievanceCategoryId", Grievance_Category.GrievanceCategoryId);
                param.Add("@p_GrievanceCategoryId_Encrypted", Grievance_Category.GrievanceCategoryId_Encrypted);
                param.Add("@p_GrievanceCategory", Grievance_Category.GrievanceCategory);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Employee_Grievance_Category", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Employee_Grievance_Category", "Setting_Employee_Grievance_Category");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region Delete
        public ActionResult Delete(int? GrievanceCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_GrievanceCategoryId", GrievanceCategoryId);
                param.Add("@p_GrievanceCategoryId_Encrypted", string.Empty);
                param.Add("@p_GrievanceCategory", string.Empty);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Employee_Grievance_Category", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Employee_Grievance_Category");
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


          