using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.ESS.Models.ESS_Warning;
//using KompassHR.Areas.Setting.Models.Setting_Warning;
using Dapper;
using KompassHR.Models;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.IO;
//using KompassHR.Areas.Setting.Models.Setting_Warning;

namespace KompassHR.Areas.ESS.Controllers.ESS_Warning
{
    public class ESS_Warning_EmployeeWarningController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        public ActionResult ESS_Warning_EmployeeWarning(string WarningEmpID_Encrypted,int? WarningEmployeeID)
        {
            try
            {
                ViewBag.AddUpdateTitle = "Add";
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 567;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                Warning_Employee Warning_Employee = new Models.ESS_Warning.Warning_Employee();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var Manager1 = Session["ManagerId1"];
                    var Manger2 = Session["ManagerId2"];
                    var HRId = Session["HRId"];
                    var EmployeeId = Session["EmployeeId"];

                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Warning_Employee";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;

                    var GetTotalWarning = "Select Isnull(Max(DocNo),0)+1 As DocNo from Warning_Employee";
                    var TotalWarning = DapperORM.DynamicQuerySingle(GetTotalWarning);
                    ViewBag.TotalWarning = TotalWarning;

                    var GetWarningThisMonthTotal = "Select Isnull(Max(DocNo),0)+1 As DocNo from Warning_Employee";
                    var GetMonthTotal = DapperORM.DynamicQuerySingle(GetWarningThisMonthTotal);
                    ViewBag.GetMonthTotal = GetMonthTotal;

                    param = new DynamicParameters();
                    var warningLastRecord = "select max(CreatedDate) As CreatedDate from Warning_Employee where EmployeerID=" + EmployeeId + " and  Deactivate = 0";
                    var LastRecored = DapperORM.DynamicQuerySingle(warningLastRecord);
                    ViewBag.LastRecored = LastRecored.CreatedDate;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "SELECT WarningID,WarningType FROM Warning_Type where Deactivate = 0 order by  WarningType");
                    var GetWarningType = DapperORM.ReturnList<Setting.Models.Setting_Warning.Warning_Type>("sp_QueryExcution", param1).ToList();
                    ViewBag.GetWarningType = GetWarningType;

                    var GetEmployee = new BulkAccessClass().AllEmployeeName();
                    ViewBag.GetEmployeeName = GetEmployee;
                }

                if (WarningEmpID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    var EmployeeId = Session["EmployeeId"];
                    param.Add("@p_WarningEmpID_Encrypted", WarningEmpID_Encrypted);
                    param.Add("@p_EmployeerID", WarningEmployeeID);                   
                    Warning_Employee = DapperORM.ReturnList<Warning_Employee>("sp_List_Warning_Employee", param).FirstOrDefault();
                    TempData["DocDate"] = Warning_Employee.DocDate;
                    
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var TotalWarning = "";
                        var MonthTotal = "";

                        var GetTotalWarning = "select isnull((select count(WarningEmployeeID) as WarningEmployeeID from Warning_Employee where Deactivate =0 and WarningEmployeeID='" + WarningEmployeeID + "'),0) as TotalWarning ";
                        var TotalWarning1 = DapperORM.DynamicQuerySingle(GetTotalWarning);
                        TotalWarning = Convert.ToString(TotalWarning1.TotalWarning);
                        TempData["TotalWarning"] = TotalWarning;

                        var GetWarningThisMonthTotal = "select isnull((select count(WarningEmpID) as WarningEmpID from Warning_Employee where Deactivate =0 and WarningEmployeeID='" + WarningEmployeeID + "' and month(DocDate)=month(getdate()) and year(DocDate)=year(getdate())   ),0) as TotalWarning ";
                        var MonthTotal1 = DapperORM.DynamicQuerySingle(GetWarningThisMonthTotal);
                        MonthTotal = Convert.ToString(MonthTotal1.TotalWarning);
                        TempData["MonthTotal"] = MonthTotal;

                        var GetDocNo = "Select Isnull(Max(DocNo),0) As DocNo from Warning_Employee where WarningEmpID_Encrypted='" + WarningEmpID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;                      
                    }
                }
                return View(Warning_Employee);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Warning_EmployeeWarning");
            }
        }

        [HttpGet]
        public ActionResult GetEmployeeWarningDetail(int WarningEmployeeID)
        {
            ViewBag.TotalWarning = "";
            ViewBag.GetMonthTotal = "";

            var TotalWarning = "";
            var MonthTotal = "";

            using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
            {
                var GetTotalWarning = "select isnull((select count(WarningEmployeeID) as WarningEmployeeID from Warning_Employee where Deactivate =0 and WarningEmployeeID='" + WarningEmployeeID + "'),0) as TotalWarning ";
                var TotalWarning1 = DapperORM.DynamicQuerySingle(GetTotalWarning);
                TotalWarning = Convert.ToString(TotalWarning1.TotalWarning);
                ViewBag.TotalWarning = TotalWarning;

                Session["TotalWarning"] = TotalWarning;

                var GetWarningThisMonthTotal = "select isnull((select count(WarningEmpID) as WarningEmpID from Warning_Employee where Deactivate =0 and WarningEmployeeID='" + WarningEmployeeID + "' and month(DocDate)=month(getdate()) and year(DocDate)=year(getdate())   ),0) as TotalWarning ";
                var MonthTotal1 = DapperORM.DynamicQuerySingle(GetWarningThisMonthTotal);
                MonthTotal = Convert.ToString(MonthTotal1.TotalWarning);
                ViewBag.GetMonthTotal = MonthTotal1;
                Session["MonthTotal1"] = MonthTotal1.TotalWarning;
            }
            var data = new
            {
                TotalWarning = TotalWarning,
                MonthTotal = MonthTotal
            };

            return Json(data, JsonRequestBehavior.AllowGet);
            //return View();
        }


        public JsonResult IsEmployeeWarningExists(int DocNo, DateTime DocDate, double WarningEmployeeID, string WarningEmpID_Encrypted, double WarningID, string Description)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var EmployeeId = Session["EmployeeId"];
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_WarningEmpID_Encrypted", WarningEmpID_Encrypted);
                    param.Add("@p_DocDate", DocDate);
                    param.Add("@p_WarningEmployeeID", WarningEmployeeID);
                    param.Add("@p_EmployeerID", EmployeeId);
                    param.Add("@p_WarningID", WarningID);
                    param.Add("@p_Description", Description);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Warning_Employee", param);
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
            catch (Exception Ex)
            {
                return Json(Ex, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult SaveUpdate(Warning_Employee Warning_Employee)
        {

            try
            {

                DynamicParameters param = new DynamicParameters();
                var EmployeeId = Session["EmployeeId"];
                var CompanyId = Session["CompanyId"];
                param.Add("@p_process", string.IsNullOrEmpty(Warning_Employee.WarningEmpID_Encrypted) ? "Save" : "Update");
                param.Add("@p_WarningEmpID", Warning_Employee.WarningEmpID);
                param.Add("@p_WarningEmpID_Encrypted", Warning_Employee.WarningEmpID_Encrypted);
                param.Add("@p_CmpId", CompanyId);
                param.Add("@p_WarningEmployeeID", Warning_Employee.WarningEmployeeID);
                param.Add("@p_EmployeerID", EmployeeId);
                param.Add("@p_DocNo", Warning_Employee.DocNo);
                param.Add("@p_DocDate", Warning_Employee.DocDate);
                param.Add("@p_WarningID", Warning_Employee.WarningID);
                param.Add("@p_Description", Warning_Employee.Description);
                param.Add("@P_Status", "Pending");
                param.Add("@p_RejectRemarks", Warning_Employee.RejectRemarks);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Warning_Employee", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["PrimaryKey_Id"] = param.Get<string>("@p_Id");

                return RedirectToAction("ESS_Warning_EmployeeWarning", "ESS_Warning_EmployeeWarning");

            }
            catch (Exception Ex)
            {

                return RedirectToAction(Ex.Message.ToString(), "ESS_Warning_EmployeeWarning");
            }

        }
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 567;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_WarningEmpID_Encrypted", "List");
                param.Add("@p_EmployeerID", EmployeeId);
                var data = DapperORM.DynamicList("sp_List_Warning_Employee", param);
                ViewBag.GetEmployeeWarningList = data;
                return View(data);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Warning_EmployeeWarning");
            }
        }

        public ActionResult Delete(string WarningEmpID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_WarningEmpID_Encrypted", WarningEmpID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Warning_Employee", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Warning_EmployeeWarning");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Warning_EmployeeWarning");
            }

        }
    }
}