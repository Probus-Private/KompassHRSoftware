using Dapper;
using KompassHR.Areas.ESS.Models.ESS_EmployeeGrievance;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Employee_Grievance
{
    public class ESS_Employee_RaiseGrievanceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);



        #region ESS_Employee_RaiseGrievance Main View 
        [HttpGet]
        // GET: ESS/ESS_Employee_RaiseGrievance
        public ActionResult ESS_Employee_RaiseGrievance(string RaiseGrievanceId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 675;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                ViewBag.GrievanceSubCategory = "";
                ViewBag.SubCatDescription = "";

                Employee_RaiseGrievance RaiseGrievance = new Employee_RaiseGrievance();
                DynamicParameters param = new DynamicParameters();

                // 🔹 Load Categories
                DynamicParameters paramCategory = new DynamicParameters();
                var GetCategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_GetGrievanceCategoryDropdown", paramCategory).ToList();
                ViewBag.CategoryName = GetCategoryName;

                var GetDocNo = "SELECT ISNULL(MAX(DocId), 0) + 1 AS DocId FROM Employee_RaiseGrievance";
                var result = DapperORM.DynamicQuerySingle(GetDocNo);
                var DocId = result.DocId;
                ViewBag.DocId = DocId;
                RaiseGrievance.DocId = DocId;

                if (RaiseGrievanceId_Encrypted != null)
                {
                    // ---------------- UPDATE ----------------
                    ViewBag.AddUpdateTitle = "Update";

                    param.Add("@p_RaiseGrievanceId_Encrypted", RaiseGrievanceId_Encrypted);
                    RaiseGrievance = DapperORM.ReturnList<Employee_RaiseGrievance>("sp_List_Employee_RaiseGrievance", param).FirstOrDefault();
                    ViewBag.DocId = RaiseGrievance?.DocId ?? 0;

                    DynamicParameters paramSubCategory = new DynamicParameters();
                    paramSubCategory.Add("@p_GrievanceCategoryId", RaiseGrievance.GrievanceCategoryId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetGrievanceSubCategoryDropdown", paramSubCategory).ToList();
                    ViewBag.GrievanceSubCategory = data;

                    TempData["SelectedFile"] = RaiseGrievance.GrievanceFilePath;
                    TempData["FileName"] = RaiseGrievance.GrievanceFilePath;
                }
                return View(RaiseGrievance);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion



        #region GetGrievanceSubCategory
        [HttpGet]
        public ActionResult GetGrievanceSubCategory(int GrievanceCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_GrievanceCategoryId", GrievanceCategoryId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetGrievanceSubCategoryDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region  GetSubCatDescription
        [HttpGet]
        public ActionResult GetSubCatDescription(int GrievanceSubCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //DynamicParameters param = new DynamicParameters();
                var GetDescription = "select Description from Employee_Grievance_SubCategory where GrievanceSubCategoryId='" + GrievanceSubCategoryId + "';";
                var Description = DapperORM.DynamicQuerySingle(GetDescription);
                var data = Description.Description;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region IsRaiseGrievanceExists
        public ActionResult IsRaiseGrievanceExists(double GrievanceCategoryId, string GrievanceSubCategoryId, string RaiseGrievanceId_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_GrievanceCategoryId", GrievanceCategoryId);
                    param.Add("@p_GrievanceSubCategoryId", GrievanceSubCategoryId);
                    param.Add("@p_RaiseGrievanceId_Encrypted", RaiseGrievanceId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    var Result = DapperORM.ExecuteReturn("sp_SUD_RaiseGrievance", param);
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
        public ActionResult SaveUpdate(Employee_RaiseGrievance RaiseGrievance, HttpPostedFileBase GrievanceFilePath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(RaiseGrievance.RaiseGrievanceId_Encrypted) ? "Save" : "Update");
                param.Add("@p_RaiseGrievanceId", RaiseGrievance.RaiseGrievanceId);
                param.Add("@p_RaiseGrievanceId_Encrypted", RaiseGrievance.RaiseGrievanceId_Encrypted);
                param.Add("@p_GrievanceCategoryId", RaiseGrievance.GrievanceCategoryId);
                param.Add("@p_GrievanceSubCategoryId", RaiseGrievance.GrievanceSubCategoryId);
                param.Add("@p_SubCatDescription", RaiseGrievance.SubCatDescription);
                param.Add("@p_DocId", RaiseGrievance.DocId);
                param.Add("@p_Description", RaiseGrievance.Description);

                if (GrievanceFilePath != null)
                {
                    param.Add("@p_GrievanceFilePath", GrievanceFilePath.FileName);
                }
                else if (!string.IsNullOrEmpty(RaiseGrievance.RaiseGrievanceId_Encrypted))
                {
                    param.Add("@p_GrievanceFilePath", RaiseGrievance.GrievanceFilePath); // Retain existing file name
                }
                else
                {
                    param.Add("@p_GrievanceFilePath", "");
                }

                param.Add("@p_GrievanceRaiseDate", RaiseGrievance.GrievanceRaiseDate);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_RaiseGrievance", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");

                var P_Id = param.Get<string>("@p_Id");
                if (P_Id != null)
                {

                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='RaiseGrievance'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = Path.Combine(GetFirstPath, P_Id.ToString());

                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (GrievanceFilePath != null)
                    {
                        string ImgGrievanceFilePath = "";
                        ImgGrievanceFilePath = FirstPath + GrievanceFilePath.FileName;
                        GrievanceFilePath.SaveAs(ImgGrievanceFilePath);
                    }
                }
                return RedirectToAction("GetList", "ESS_Employee_RaiseGrievance");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 675;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ESS" });
                }
                param.Add("@p_RaiseGrievanceId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);

                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Employee_RaiseGrievance", param).ToList();
                ViewBag.GetRaiseGrievanceList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadFile
        public ActionResult DownloadFile(int? RaiseGrievanceId, string fileName)
        {
            try
            {
                if (RaiseGrievanceId == null || string.IsNullOrEmpty(fileName))
                {
                    return HttpNotFound("File Not Found");
                }
                var GetDocPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin='RaiseGrievance'");
                string basePath = GetDocPath.DocInitialPath;
                string fullFilePath = Path.Combine(basePath, RaiseGrievanceId.ToString(), fileName);
                if (System.IO.File.Exists(fullFilePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fullFilePath);
                    return File(fileBytes, MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    return HttpNotFound("File not found at path: " + fullFilePath);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int? RaiseGrievanceId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "ESS" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_RaiseGrievanceId", RaiseGrievanceId);
                param.Add("@p_RaiseGrievanceId_Encrypted", string.Empty);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_RaiseGrievance", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Employee_RaiseGrievance");
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
