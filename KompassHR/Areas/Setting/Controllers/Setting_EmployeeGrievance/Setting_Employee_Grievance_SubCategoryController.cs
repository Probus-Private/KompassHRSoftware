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
    public class Setting_Employee_Grievance_SubCategoryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Setting_Employee_Grievance_SubCategory Main View 
        [HttpGet]
        // GET: Setting/Setting_Employee_Grievance_SubCategory
        public ActionResult Setting_Employee_Grievance_SubCategory(string GrievanceSubCategoryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 577;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters paramCategory = new DynamicParameters();
                var GetCategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_GetGrievanceCategoryDropdown", paramCategory).ToList();
                var CID = GetCategoryName[0].Id;
                ViewBag.CategoryName = GetCategoryName;

                Employee_Grievance_SubCategory GrievanceSubCategory = new Employee_Grievance_SubCategory();
                if (GrievanceSubCategoryId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_GrievanceSubCategoryId_Encrypted", GrievanceSubCategoryId_Encrypted);
                    GrievanceSubCategory = DapperORM.ReturnList<Employee_Grievance_SubCategory>("sp_List_Employee_Grievance_SubCategory", param).FirstOrDefault();
                }
                return View(GrievanceSubCategory);
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
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 673;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_GrievanceSubCategoryId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Employee_Grievance_SubCategory", param).ToList();
                ViewBag.GetGrievanceSubCategoryList = data;


                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsGrievanceSubCategoryExists
        public ActionResult IsGrievanceSubCategoryExists(double GrievanceCategoryId, string GrievanceSubCategory, string GrievanceSubCategoryId_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_GrievanceCategoryId", GrievanceCategoryId);
                    param.Add("@p_GrievanceSubCategory", GrievanceSubCategory);
                    param.Add("@p_GrievanceSubCategoryId_Encrypted", GrievanceSubCategoryId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    var Result = DapperORM.ExecuteReturn("sp_SUD_GrievanceSubCategory", param);
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
        public ActionResult SaveUpdate(Employee_Grievance_SubCategory SubCategory)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(SubCategory.GrievanceSubCategoryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_GrievanceSubCategoryId", SubCategory.GrievanceSubCategoryId);
                param.Add("@p_GrievanceSubCategoryId_Encrypted", SubCategory.GrievanceSubCategoryId_Encrypted);
                param.Add("@p_GrievanceCategoryId", SubCategory.GrievanceCategoryId);
                param.Add("@p_GrievanceSubCategory", SubCategory.GrievanceSubCategory);
                param.Add("@p_Description", SubCategory.Description);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_GrievanceSubCategory", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Employee_Grievance_SubCategory", "Setting_Employee_Grievance_SubCategory");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int? GrievanceSubCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "Setting" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_GrievanceSubCategoryId", GrievanceSubCategoryId);
                param.Add("@p_GrievanceSubCategoryId_Encrypted", string.Empty);
                param.Add("@p_GrievanceSubCategory", string.Empty);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_GrievanceSubCategory", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Employee_Grievance_SubCategory");
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