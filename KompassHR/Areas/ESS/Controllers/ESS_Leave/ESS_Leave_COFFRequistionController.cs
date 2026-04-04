using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Leave;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Leave
{
    public class ESS_Leave_COFFRequistionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Leave_COFFRequistion

        #region ESS_Leave_COFFRequistion
        public ActionResult ESS_Leave_COFFRequistion()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 293;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_CoffRequest";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;

                DynamicParameters paramShift = new DynamicParameters();
                paramShift.Add("@p_EmployeeID", Session["EmployeeId"]);
                ViewBag.GetShiftList = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", paramShift);

                DynamicParameters paramDays = new DynamicParameters();
                paramDays.Add("@p_Employeeid", Session["EmployeeId"]);
                var GetGeneratedDays = DapperORM.DynamicList("sp_GetCoffBalance", paramDays);
                ViewBag.GetGeneratedDays = GetGeneratedDays;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region COFFRequistionExists
        public ActionResult COFFRequistionExists(DateTime FromDate, DateTime ToDate, string CoffRequestID_Encrypted, string CoffRequestDayType, List<Coff_GeneratedID> CoffRequestIDRecord)
        {
            try
            {
                Session["GetErrorMessage"] = "";
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                List<int> Arraylist = new List<int>() { };
                string result = "";
                for (var i = 0; i < CoffRequestIDRecord.Count; i++)
                {
                    result = result + CoffRequestIDRecord[i].CoffGeneratedID + ",";
                }
                result = result.TrimEnd(',');
                Session["CoffRequestIDRecord"] = result;
                var GetFromDate = FromDate.ToString("yyyy-MM-dd");
                var GetToDate = ToDate.ToString("yyyy-MM-dd");
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CoffRequestDayType", CoffRequestDayType);
                param.Add("@p_CoffRequestID_Encrypted", CoffRequestID_Encrypted);
                param.Add("@p_FromDate", GetFromDate);
                param.Add("@p_ToDate", GetToDate);
                param.Add("@p_CoffRequesterEmployeeID", Session["EmployeeId"]);
                param.Add("@p_CoffGenerationIDs", result);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_COFFRequistion", param);
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate  

        public ActionResult SaveUpdate(Atten_CoffRequest CoffRequest)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 293;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(CoffRequest.CoffRequestID_Encrypted) ? "Save" : "Update");
                param.Add("@p_DocNo", CoffRequest.DocNo);
                param.Add("@p_DocDate", CoffRequest.DocDate);
                param.Add("@p_CoffRequesterEmployeeID", Session["EmployeeId"]);
                param.Add("@p_FromDate", CoffRequest.FromDate);
                param.Add("@p_CoffShiftId", CoffRequest.CoffShiftId);
                param.Add("@p_ToDate", CoffRequest.ToDate);
                param.Add("@p_CoffRequestDayType", CoffRequest.CoffRequestDayType);
                param.Add("@p_Reason", CoffRequest.Reason);
                param.Add("@p_CoffGenerationIDs", Session["CoffRequestIDRecord"]);

                param.Add("@p_RequestFrom", "Web");

                param.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_COFFRequistion", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                //=========== START NOTIFICATION SEND CODE ================================
                string customerCode = Convert.ToString(Session["ESSCustomerCode"]);
                //string customerCode = "P_K1005";
                int empId = Convert.ToInt32(Session["EmployeeId"]);
                string empName = Convert.ToString(Session["EmployeeName"]);

                // Fetch data
                var Sendmessage = DapperORM.DynamicQueryList("SELECT * FROM tbl_NotificationMessage WHERE Origin='COff'").FirstOrDefault();
                var approver = DapperORM.DynamicQueryList($@"SELECT ReportingManager1 AS EmpId FROM Mas_Employee_Reporting WHERE ReportingModuleId=8 AND Deactivate=0 AND ApproverLevel=1 AND ReportingEmployeeID={empId}").FirstOrDefault();
                if (Sendmessage != null && approver != null)
                {
                    var request = new
                    {
                        Title = $"🔔 {Sendmessage.MessageTitle}",
                        Body = $"👤 {Sendmessage.MessageBody} {empName}",
                        NotifyEmpId = Convert.ToInt32(approver.EmpId),
                        CreatedBy = empName,
                        RequestType = Sendmessage.Origin
                    };
                    string response = new NotificationService().SendNotification(customerCode, request);
                }
                //=========== END NOTIFICATION SEND CODE ================================
                return RedirectToAction("ESS_Leave_COFFRequistion", "ESS_Leave_COFFRequistion");
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
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 293;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_Qry", "and Atten_CoffRequest.CoffRequesterEmployeeID=" + Session["EmployeeId"] + "");
                var data = DapperORM.DynamicList("sp_List_Atten_CoffRequest", param);
                ViewBag.GetCoffRequest = data;

                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                                                where reportingmoduleid = 1 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region LeaveDelete
        [HttpGet]
        public ActionResult Delete(string CoffRequestID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 293;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_CoffRequestID_Encrypted", CoffRequestID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_COFFRequistion", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Leave_COFFRequistion");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region CoffRequest Cancel Request Request function
        [HttpGet]
        public ActionResult CancelRequest(int? GetCoffRequestID, int ddlManagerId, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", GetCoffRequestID);
                param.Add("@p_ManagerID", ddlManagerId);
                param.Add("@p_CancelRemark", Remark);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Tra_RequestCancel", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region RequestStatus
        [HttpGet]
        public ActionResult RequestStatus(int DocId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_DocID", DocId);
                param.Add("@p_Origin", "Atten_CoffRequest");
                var data = DapperORM.ExecuteSP<dynamic>("sp_RequestTimeLine", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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