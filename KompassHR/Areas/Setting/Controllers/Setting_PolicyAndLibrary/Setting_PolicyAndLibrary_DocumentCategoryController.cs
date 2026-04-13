using Dapper;
using KompassHR.Models;
using KompassHR.Areas.Setting.Models.Setting_PolicyAndLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_PolicyAndLibrary
{
    public class Setting_PolicyAndLibrary_DocumentCategoryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: PrimeSetting/DocumentCategory
        public ActionResult Setting_PolicyAndLibrary_DocumentCategory(string CompanyDocumentCategoryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_CompanyDocumentCategory mas_CompanyDocumentCategory = new Mas_CompanyDocumentCategory();
                if (CompanyDocumentCategoryId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_CompanyDocumentCategoryId_Encrypted", CompanyDocumentCategoryId_Encrypted);
                    mas_CompanyDocumentCategory = DapperORM.ReturnList<Mas_CompanyDocumentCategory>("sp_List_Mas_CompanyDocumentCategory", param).FirstOrDefault();
                }
                return View(mas_CompanyDocumentCategory);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        public ActionResult IsDocumentCategoryExists(string DocumentCategoryName, string CompanyDocumentCategoryIdEncrypted)
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
                    param.Add("@p_CompanyDocumentCategoryId_Encrypted", CompanyDocumentCategoryIdEncrypted);
                    param.Add("@p_DocumentCategoryName", DocumentCategoryName);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyDocumentCategory", param);
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
        public ActionResult SaveUpdate(Mas_CompanyDocumentCategory ComapnyCategory)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(ComapnyCategory.CompanyDocumentCategoryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_CompanyDocumentCategoryId", ComapnyCategory.CompanyDocumentCategoryId);
                param.Add("@p_CompanyDocumentCategoryId_Encrypted", ComapnyCategory.CompanyDocumentCategoryId_Encrypted);
                param.Add("@p_DocumentCategoryName", ComapnyCategory.DocumentCategoryName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyDocumentCategory", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_PolicyAndLibrary_DocumentCategory", "Setting_PolicyAndLibrary_DocumentCategory");
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
                param.Add("@p_CompanyDocumentCategoryId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Mas_CompanyDocumentCategory", param);
                ViewBag.GetListDocumentCategory = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult Delete(string CompanyDocumentCategoryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_CompanyDocumentCategoryId_Encrypted", CompanyDocumentCategoryId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyDocumentCategory", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_PolicyAndLibrary_DocumentCategory");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}