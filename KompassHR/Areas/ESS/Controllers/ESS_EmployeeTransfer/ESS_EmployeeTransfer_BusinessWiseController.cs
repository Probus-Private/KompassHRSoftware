using Dapper;
using KompassHR.Areas.ESS.Models.ESS_EmployeeTransfer;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_EmployeeTransfer
{
    public class ESS_EmployeeTransfer_BusinessWiseController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_EmployeeTransfer_BusinessWise
        #region Main View
        public ActionResult ESS_EmployeeTransfer_BusinessWise()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 405;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                var CID = GetComapnyName[0].Id;
                ViewBag.CompanyName = GetComapnyName;

                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Trans_Businessunit";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;


                ViewBag.ToBranchName = "";
                // DynamicParameters param = new DynamicParameters();
                //  param.Add("@query", "select mas_branch.BranchID as Id ,mas_branch.BranchName as Name from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid= " + Session["EmployeeId"] + " and mas_branch.BranchId=UserBranchMapping.branchid and mas_branch.Deactivate=0 and UserBranchMapping.IsActive=1 order by Name");
                //ViewBag.ToBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //param.Add("@p_employeeid", Session["EmployeeId"]);
                //param.Add("@p_CmpId", Session["CompanyId"]);
                //ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                //DynamicParameters paramEmp = new DynamicParameters();
                //paramEmp.Add("@p_employeeid", Session["EmployeeId"]);
                ////paramEmp.Add("@p_CmpId", Session["CompanyId"]);
                //ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_GetEmployeeNameDropdown", paramEmp).ToList();

                //DynamicParameters paramHR = new DynamicParameters();
                //paramHR.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeDepartmentID in (Select DepartmentId from Mas_Department where DepartmentName like '%HR%')  and EmployeeId<>1 and Mas_Employee.ContractorID=1 order by Name");
                //ViewBag.HREmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramHR).ToList();
                ViewBag.HREmployeeName = "";
                ViewBag.BranchName = "";
                ViewBag.EmployeeName = "";

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsTransferBusinessExists(DateTime DocDate, int? TransferEmployeeId, int? TransferToBranchId)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_DocDate", DocDate);
                param.Add("@p_TransferEmployeeId", TransferEmployeeId);
                param.Add("@p_TransferToBranchId", TransferToBranchId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Trans_BusinessUnit", param);
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
        [HttpPost]
        public ActionResult SaveUpdate(Trans_BusinessUnit TransferUnit)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Save");
                param.Add("@p_TransferBusinessUnitId_Encrypted", TransferUnit.TransferBusinessUnitId_Encrypted);
                param.Add("@p_DocNo", TransferUnit.DocNo);
                param.Add("@p_DocDate", TransferUnit.DocDate);
                param.Add("@p_TransferEmployeeId", TransferUnit.TransferEmployeeId);
                param.Add("@p_TransferToBranchId", TransferUnit.TransferToBranchId);
                param.Add("@p_TransferReportingDate", TransferUnit.TransferReportingDate);
                param.Add("@p_TransferReportingHRId", TransferUnit.TransferReportingHRId);
                param.Add("@p_TransferOrderBy", Session["EmployeeId"]);
                param.Add("@p_TransferReason", TransferUnit.TransferReason);
                param.Add("@p_TransferToCmpID", TransferUnit.TransferToCmpID);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Trans_BusinessUnit", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_EmployeeTransfer_BusinessWise", "ESS_EmployeeTransfer_BusinessWise");

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 405;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_Qry", "and Trans_BusinessUnit.TransferOrderBy=" + Session["EmployeeId"] + "");
                var Trans_BusinessUnitList = DapperORM.DynamicList("sp_List_Trans_BusinessUnit", param);

                ViewBag.Trans_BusinessUnitList = Trans_BusinessUnitList;


                //DynamicParameters ParamManager = new DynamicParameters();
                //ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                //                                where reportingmoduleid = 1 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                //var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                //ViewBag.GetManagerEmployee = Getdata;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetEmployeeName(int BranchId, int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramEmp = new DynamicParameters();
                paramEmp.Add("@p_BranchId", BranchId);
                //paramEmp.Add("@p_CmpId", Session["CompanyId"]);
                var GetEmployeeNameDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetEmployeeNameDropdown", paramEmp).ToList();

                DynamicParameters param = new DynamicParameters();
                // param.Add("@p_employeeid", Session["EmployeeId"]);
                //    param.Add("@p_CmpId", CmpId);


                //param.Add("@query", "Select mas_branch.BranchID as Id ,mas_branch.BranchName as Name from Mas_Branch where Mas_Branch.Deactivate=0 and mas_branch.BranchID<>" + BranchId + " order by Name");
                param.Add("@query", "Select mas_branch.BranchID AS Id, mas_branch.BranchName AS Name FROM Mas_Branch WHERE  Mas_Branch.Deactivate = 0 AND CmpId = '" + CmpId + "' AND BranchID <> '" + BranchId + "' ORDER BY BranchName");

                // var ToBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                var ToBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(new { ToBranchName = ToBranchName, GetEmployeeNameDropdown = GetEmployeeNameDropdown }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetHREmployeeName(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramHR = new DynamicParameters();
                paramHR.Add("@query", "select Mas_Employee.EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee left join FNF_HRMapping on FNF_HRMapping.EmployeeID=Mas_Employee.EmployeeId where Mas_Employee.Deactivate=0 and  FNF_HRMapping.BusinessUnitID=" + BranchId + "and Mas_Employee.EmployeeId<>1 and Mas_Employee.ContractorID=1 order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramHR).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string TransferBusinessUnitId_Encrypted, int TransferBusinessUnitId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TransferBusinessUnitId", TransferBusinessUnitId);
                param.Add("@p_TransferBusinessUnitId_Encrypted", TransferBusinessUnitId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Trans_BusinessUnit", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_EmployeeTransfer_BusinessWise");
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