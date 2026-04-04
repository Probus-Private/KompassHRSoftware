using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TicketManagement;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.Setting.Controllers.Setting_TicketManagement
{
    public class Setting_TicketManagement_SubTicketCategoryController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: Setting/Setting_TicketManagement_SubTicketCategory
        public ActionResult Setting_TicketManagement_SubTicketCategory(string TicketSubCategoryID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 767;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Ticket_SubCategory SubCategory = new Ticket_SubCategory();
                ViewBag.AddUpdateTitle = "Add";

                param.Add("@query", "select TicketCategoryID as Id,TicketCategoryName as Name  from Ticket_Category where Deactivate=0");
                var CategoryList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.CategoryList = CategoryList;

                if (TicketSubCategoryID_Encrypted != null)
                {
                    DynamicParameters param2= new DynamicParameters();

                    ViewBag.AddUpdateTitle = "Update";
                    param2.Add("@p_TicketSubCategoryID_Encrypted", TicketSubCategoryID_Encrypted);
                    SubCategory = DapperORM.ReturnList<Ticket_SubCategory>("sp_List_TicketSubCategory", param2).FirstOrDefault();
                }
                return View(SubCategory);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult IsSubTicketCategoryExists(string TicketSubCategoryID_Encrypted, string TicketSubCategoryName, int TicketCategory_Id)
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
                    param.Add("@p_TicketSubCategoryID_Encrypted", TicketSubCategoryID_Encrypted);
                    param.Add("@p_TicketSubCategoryName", TicketSubCategoryName);
                    param.Add("@p_TicketCategory_Id", TicketCategory_Id);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_SubCategory", param);
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


        [HttpPost]
        public ActionResult SaveUpdate(Ticket_SubCategory Ticket_SubCategory)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int Cmpid = Convert.ToInt32(Session["CompanyId"]);

                param.Add("@p_process", string.IsNullOrEmpty(Ticket_SubCategory.TicketSubCategoryID_Encrypted) ? "Save" : "Update");
                param.Add("@p_TicketSubCategoryID_Encrypted", Ticket_SubCategory.TicketSubCategoryID_Encrypted);
                param.Add("@P_TicketSubCategoryName", Ticket_SubCategory.TicketSubCategoryName);
                param.Add("@P_TicketCategory_Id", Ticket_SubCategory.TicketCategory_Id);
                param.Add("@p_CmpID", Cmpid);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Ticket_SubCategory", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_TicketManagement_SubTicketCategory", "Setting_TicketManagement_SubTicketCategory");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 767;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_TicketSubCategoryID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_TicketSubCategory", param);
                ViewBag.GetTicketSubCategoryList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult Delete(string TicketSubCategoryID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TicketSubCategoryID", TicketSubCategoryID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_SubCategory", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TicketManagement_SubTicketCategory");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}