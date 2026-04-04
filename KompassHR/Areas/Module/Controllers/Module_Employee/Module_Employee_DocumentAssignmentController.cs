using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_DocumentAssignmentController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        [HttpGet]
        #region Document Assignment Main View 
        public ActionResult Module_Employee_DocumentAssignment(string DocAssignmentMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 905;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from DocumentAssignment_Master where deactivate=0";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo.DocNo;

                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch;

                var results = DapperORM.DynamicQueryMultiple(@"SELECT  DepartmentId as Id,DepartmentName as Name FROM Mas_Department WHERE Deactivate =0;");
                ViewBag.DepatmentList = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                Module_Employee_DocumentAssignment Module_Employee_DocumentAssignment = new Module_Employee_DocumentAssignment();
                if (DocAssignmentMasterId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@P_DocAssignmentMasterId_Encrypted", DocAssignmentMasterId_Encrypted);
                    Module_Employee_DocumentAssignment = DapperORM.ReturnList<Module_Employee_DocumentAssignment>("sp_List_Onboarding_DocumentAssignment", param).FirstOrDefault();
                    ViewBag.DocNo = Module_Employee_DocumentAssignment.DocNo;
                    TempData["DueDate"] = Module_Employee_DocumentAssignment.DueDate.ToString("yyyy-MM-dd");

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_CmpId", Module_Employee_DocumentAssignment.CmpId);
                    paramList.Add("@p_BranchId", Module_Employee_DocumentAssignment.BranchId);
                    paramList.Add("@p_DepartmentId", Module_Employee_DocumentAssignment.DepartmentId);
                    paramList.Add("@p_DocAssignmentMasterId", Module_Employee_DocumentAssignment.DocAssignmentMasterId);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_Onbording_GetDocumentAssignEmployeeList", paramList).ToList();
                    ViewBag.GetDocumentAssignEmployeeList = data;

                    //GET BRANCH NAME
                    var Branch1 = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(Module_Employee_DocumentAssignment.CmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch1;
                }
                return View(Module_Employee_DocumentAssignment);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get Branch Name
        [HttpGet]
        public ActionResult GetMonthlyBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Employee details
        [HttpGet]
        public ActionResult GetEmployeeDetails(int CmpId, string BranchId, string DepartmentId,int? DocAssignmentMasterId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters paramList = new DynamicParameters();
            paramList.Add("@p_CmpId", CmpId);
            paramList.Add("@p_BranchId", BranchId);
            paramList.Add("@p_DepartmentId", DepartmentId);
            paramList.Add("@p_DocAssignmentMasterId", DocAssignmentMasterId);
            var data = DapperORM.ExecuteSP<dynamic>("sp_Onbording_GetDocumentAssignEmployeeList", paramList).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region IsDocumentAssignExists
        public ActionResult IsDocumentAssignExists(int DocAssignmentMasterId, string AssignmentTitle)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@P_DocAssignmentMasterId", DocAssignmentMasterId);
                    param.Add("@p_AssignmentTitle", AssignmentTitle);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_DocumentAssignment", param);
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
        public ActionResult SaveUpdate(List<DocumentAssginedEmployee> EmployeeList, Module_Employee_DocumentAssignment Module_Employee_DocumentAssignment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var param = new DynamicParameters();
                param.Add("@p_process", Module_Employee_DocumentAssignment.DocAssignmentMasterId > 0 ? "Update" : "Save");
                param.Add("@p_DocAssignmentMasterId", Module_Employee_DocumentAssignment.DocAssignmentMasterId);
                param.Add("@p_DocNo", Module_Employee_DocumentAssignment.DocNo);
                param.Add("@p_DocDate", Module_Employee_DocumentAssignment.DocDate);
                param.Add("@p_AssignmentTitle", Module_Employee_DocumentAssignment.AssignmentTitle);
                param.Add("@p_Description", Module_Employee_DocumentAssignment.Description);
                param.Add("@p_Instruction", Module_Employee_DocumentAssignment.Instruction);
                param.Add("@p_DueDate", Module_Employee_DocumentAssignment.DueDate);
                param.Add("@p_CmpId", Module_Employee_DocumentAssignment.CmpId);
                param.Add("@p_BranchId", Module_Employee_DocumentAssignment.BranchId);
                param.Add("@p_DepartmentId", Module_Employee_DocumentAssignment.DepartmentId);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                var data = DapperORM.ExecuteReturn("sp_SUD_DocumentAssignment", param);
                int? p_Id = param.Get<int?>("@p_Id");
                int masterId = Convert.ToInt32(Module_Employee_DocumentAssignment.DocAssignmentMasterId) > 0 ? Convert.ToInt32(Module_Employee_DocumentAssignment.DocAssignmentMasterId) : (p_Id ?? 0);
                StringBuilder strBuilder = new StringBuilder();

                if (Module_Employee_DocumentAssignment.DocAssignmentMasterId > 0)
                {
                    DapperORM.DynamicQuerySingle("DELETE FROM DocumentAssignment_Details WHERE DocAssignmentMasterId='" + Module_Employee_DocumentAssignment.DocAssignmentMasterId + "'");
                }
                for (var i = 0; i < EmployeeList.Count; i++)
                {
                    var qry = "INSERT INTO dbo.DocumentAssignment_Details " +
                                 "(DocAssignmentMasterId, EmployeeId,SubmissionStatus,IsActive,Deactivate,CreatedBy, CreatedDate, MachineName) " +
                                 "VALUES ('" + masterId + "', '" + EmployeeList[i].EmployeeId + "','Pending',1,0, '" + Session["EmployeeName"] + "', GETDATE(), '" + Dns.GetHostName() + "');";
                    strBuilder.Append(qry);
                }
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record save successfully";
                    TempData["Icon"] = "success";
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

        #region GetList
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 905;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                //param.Add("@P_TrainingPlan_MasterId_Encrypted", "List");
                param.Add("@P_DocAssignmentMasterId_Encrypted", "List");
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_List_Onboarding_DocumentAssignment", param).ToList();
                ViewBag.GetDocumentAssignmentList = GetData;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region ShowEmployeeList
        [HttpGet]
        public ActionResult ShowEmployeeList(string Process,int? CmpId, string BranchId, string DepartmentId, int? DocAssignmentMasterId,string Status)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters paramList = new DynamicParameters();
            paramList.Add("@p_Process", Process);
            paramList.Add("@p_CmpId", CmpId);
            paramList.Add("@p_BranchId", BranchId);
            paramList.Add("@p_DepartmentId", DepartmentId);
            paramList.Add("@p_DocAssignmentMasterId", DocAssignmentMasterId);
            paramList.Add("@p_Status", Status);
            var data = DapperORM.ReturnList<dynamic>("sp_Onbording_GetUploadDocumentEmployeeList", paramList).ToList();
            string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete
        public ActionResult Delete(double? DocAssignmentMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_DocAssignmentMasterId", DocAssignmentMasterId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_DocumentAssignment", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "Module_Employee_DocumentAssignment");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult DeleteEmployee(List<DocumentAssginedEmployee> EmployeeList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
                var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
                var machineName = Dns.GetHostName().Replace("'", "''");
                for (var i = 0; i < EmployeeList.Count; i++)
                {
                    strBuilder.AppendLine("UPDATE dbo.DocumentAssignment_Details " +
                    "SET Deactivate = 1, " +
                    "IsActive = 0, " +
                    "ModifiedBy = '" + createdBy + "', " +
                    "ModifiedDate = GETDATE(), " +
                    "MachineName = '" + machineName + "' " +
                    "WHERE DocAssignmentDetailsId = '" + EmployeeList[i].DocAssignmentDetailsId + "';");
                }
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Deleted successfully.";
                    TempData["Icon"] = "success";
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
    }
}