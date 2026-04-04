using Dapper;
using DocumentFormat.OpenXml.Drawing;
using KompassHR.Areas.ESS.Models.ESS_EmployeeGrievance;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Employee_Grievance
{
    public class ESS_Employee_Grievance_ApproveRejectController : Controller
    {

        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        // GET: ESS/ESS_Employee_Grievance_ApproveReject
        #region Approval MAin View
        [HttpGet]
        public ActionResult ESS_Employee_Grievance_ApproveReject()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 686;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
              
                return View();
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
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 686;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ESS" });
                }
                param.Add("@p_RaiseGrievanceId_Encrypted", "List");
                param.Add("@p_ReportingManager", Session["EmployeeId"]);
            
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Employee_Grievance_ApproveReject", param).ToList();
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
        
        #region ViewForGrievance
        public ActionResult ViewForGrievance(string RaiseGrievanceId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var param = new DynamicParameters();
                param.Add("@p_RaiseGrievanceId_Encrypted", RaiseGrievanceId_Encrypted, DbType.String);

                var ResourceApproval = DapperORM.ExecuteSP<Employee_RaiseGrievance>("sp_List_Employee_Grievance_ApproveReject", param).FirstOrDefault();


                Employee_RaiseGrievance ApprovalRemark = new Employee_RaiseGrievance();
                DynamicParameters paramApprovalRemark = new DynamicParameters();
                paramApprovalRemark.Add("@query","SELECT GC.GrievanceCategory, GSC.GrievanceSubCategory, GR.GrievanceRaiseDate,GR.DocId,GR.Description,GR.Status FROM Employee_RaiseGrievance AS GR JOIN Mas_Employee AS E ON GR.RaiseGrievanceId = E.EmployeeId JOIN Employee_Grievance_Category AS GC ON GR.GrievanceCategoryId = GC.GrievanceCategoryId JOIN Employee_Grievance_SubCategory AS GSC ON GR.GrievanceSubCategoryId = GSC.GrievanceSubCategoryId WHERE GR.Deactivate = 0 AND E.Deactivate = 0 AND GR.RaiseGrievanceId_Encrypted = '"+ RaiseGrievanceId_Encrypted + "'");
                //paramApprovalRemark.Add("@RaiseGrievanceId_Encrypted", RaiseGrievanceId_Encrypted);
                 ApprovalRemark = DapperORM.ExecuteSP<Employee_RaiseGrievance>("sp_QueryExcution", paramApprovalRemark).FirstOrDefault();

                //ViewBag.ApprovalRemarkList = ApprovalRemark;
                TempData["GrievanceCategory"] = ApprovalRemark.GrievanceCategory;
                TempData["DocId"] = ApprovalRemark.DocId;
                TempData["GrievanceSubCategory"] = ApprovalRemark.GrievanceSubCategory;
                TempData["GrievanceRaiseDate"] = ApprovalRemark.GrievanceRaiseDate.ToString("dd-MM-yyyy");
                TempData["Description"] = ApprovalRemark.Description;

                return View(ApprovalRemark);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        #region Approve Reject Request function
        [HttpPost]
        public ActionResult ApproveRequest(string RaiseGrievanceId_Encrypted, string Status,string ApproveRejectRemark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 686;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();

                string Employee_Admin = " Update Employee_RaiseGrievance set Status= '" + Status + "'," +
                                                    "ApproveRejectRemark='"+ ApproveRejectRemark + "',"+
                                                    "ModifiedBy = '" + Session["EmployeeName"] + "', " +
                                                   "ModifiedDate=Getdate()," +
                                                   "MachineName='" + Dns.GetHostName().ToString() + "' " +
                                                   "Where  RaiseGrievanceId_Encrypted='" + RaiseGrievanceId_Encrypted + "'";
                strBuilder.Append(Employee_Admin);
                string abc;
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = Status == "Approved"
                        ? "Record Approved successfully"
                        : "Record Rejected successfully";
                    TempData["Icon"] = "success";
                }
                if (!string.IsNullOrEmpty(abc))
                {
                    TempData["Message"] = abc;
                    TempData["Icon"] = "error";
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

        #region ApproveRejectGetList
        public ActionResult ApproveRejectGetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 686;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ESS" });
                }
                param.Add("@p_RaiseGrievanceId_Encrypted", "List");
                param.Add("@p_ReportingManager", Session["EmployeeId"]);

                var data = DapperORM.ExecuteSP<dynamic>("sp_List_GrievanceApproveReject_Employee", param).ToList();
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
            string fullFilePath = System.IO.Path.Combine(basePath, RaiseGrievanceId.ToString(), fileName);
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
    }
}