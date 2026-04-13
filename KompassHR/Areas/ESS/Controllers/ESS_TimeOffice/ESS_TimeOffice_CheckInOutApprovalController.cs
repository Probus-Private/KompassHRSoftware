using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_CheckInOutApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region ESS_TimeOffice_CheckInOutApproval
        public ActionResult ESS_TimeOffice_CheckInOutApproval(CheckInOutApproval OBJCheckInOut)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 769;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //param.Add("@query", "Select Distinct(Mas_Employee_Reporting.ReportingEmployeeID) as Id,  Concat(Mas_Employee.EmployeeName ,' - ', Mas_Employee.EmployeeNo) as Name  from Mas_Employee_Reporting Inner Join Mas_Employee on Mas_Employee.EmployeeId = Mas_Employee_Reporting.ReportingEmployeeID "+
                //                      "where Mas_Employee_Reporting.ReportingManager1 = " + Session["EmployeeId"] + " and Mas_Employee_Reporting.Deactivate = 0 and Mas_Employee.EmployeeLeft = 0 and Mas_Employee_Reporting.ReportingModuleID=1");
                //var GetEmployees = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                DynamicParameters paramEmp = new DynamicParameters();
                paramEmp.Add("@p_employeeid", Session["EmployeeId"]);
                ViewBag.GetEmployees = DapperORM.ReturnList<AllDropDownBind>("sp_GetCheckInOutEmployeeDropdown", paramEmp).ToList();
               
                if ((OBJCheckInOut.FromDate != DateTime.MinValue) && (OBJCheckInOut.ToDate != DateTime.MinValue))
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_Employeeid", OBJCheckInOut.EmployeeId);
                    paramList.Add("@p_FromDate", OBJCheckInOut.FromDate);
                    paramList.Add("@p_ToDate", OBJCheckInOut.ToDate);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_List_AttenCheckInOut_ForApproval", paramList).ToList();
                    ViewBag.CheckInOutApproveRejectList = data;
                }
                else
                {
                    ViewBag.CheckInOutList = "";
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

        #region  Multuple Multiple Approve Reject Request function
        [HttpPost]
        public ActionResult MultipleApproveRejectRequest(ApproveRejectCheckInOut model)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DataTable tbl_AttenCheckInOut_ApporveReject = new DataTable();
                tbl_AttenCheckInOut_ApporveReject.Columns.Add("CheckInOutId", typeof(double));
                tbl_AttenCheckInOut_ApporveReject.Columns.Add("Status", typeof(string));
                tbl_AttenCheckInOut_ApporveReject.Columns.Add("Remark", typeof(string));

                foreach (var item in model.ObjCheckInOut)
                {
                    DataRow dr = tbl_AttenCheckInOut_ApporveReject.NewRow();
                    dr["CheckInOutId"] = Convert.ToDouble(item.CheckInOutId);
                    dr["Status"] = Convert.ToString(item.Status);
                    dr["Remark"] = Convert.ToString(item.Remark);
                    tbl_AttenCheckInOut_ApporveReject.Rows.Add(dr);
                }

                DynamicParameters ARparam = new DynamicParameters();
                ARparam.Add("@tbl_AttenCheckInOut_ApporveReject", tbl_AttenCheckInOut_ApporveReject.AsTableValuedParameter("tbl_AttenCheckInOut_ApporveReject"));
                ARparam.Add("@p_EmployeeId", Session["EmployeeId"]);
                ARparam.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                ARparam.Add("@p_MachineName", Dns.GetHostName().ToString());
                ARparam.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                ARparam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter  
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_AttenCheckInOut_ApporveReject", ARparam).ToList();
                TempData["Message"] = ARparam.Get<string>("@p_msg");
                TempData["Icon"] = ARparam.Get<string>("@p_Icon");
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetApproveRejectList(CheckInOut CheckInOut)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 769;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramList1 = new DynamicParameters();
                paramList1.Add("@p_Employeeid", Session["EmployeeId"]);
                paramList1.Add("@p_Process", "ApproveRejectList");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_AttenCheckInOut_ForApproval", paramList1).ToList();
                ViewBag.CheckInOutApproveRejectList = data;
                return View(CheckInOut);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetClientLatitudeAndLangitude
        public ActionResult GetClientLatitudeAndLangitude(int GetLocationName)
        {
            try
            {
                var GetLocationCredentials = DapperORM.DynamicQuerySingle("Select  LocationLatitude,LocationLongitude,LocationAreaRange from Mas_LocationRegistration where LocationRegistrationId=" + GetLocationName + "");
                return Json(GetLocationCredentials, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        public ActionResult ViewSelfie(string filePath, int EmployeeId)
        {
            if (string.IsNullOrWhiteSpace(filePath) || filePath.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                return HttpNotFound("Invalid file name.");
            }
            //var EmpId = Convert.ToInt32(Session["EmployeeId"]);
            //var GetEmpNo = DapperORM.DynamicQuerySingle("select Employeeno from mas_employee where employeeid='"+ EmpId + "'");
            //var GetNo = GetEmpNo.Employeeno;
            string directoryPath = "";
            var GetPath = DapperORM.DynamicQuerySingle("select * from Tool_Documnet_DirectoryPath where DocOrigin='Checkinout'");
            directoryPath = GetPath.DocInitialPath;
            string fullPath = Path.Combine(directoryPath, Convert.ToString(EmployeeId), filePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return HttpNotFound("File not found.");
            }

            string contentType = MimeMapping.GetMimeMapping(fullPath);
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, contentType);
        }
    }
}