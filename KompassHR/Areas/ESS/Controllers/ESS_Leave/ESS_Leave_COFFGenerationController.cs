using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Leave;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Leave
{
    public class ESS_Leave_COFFGenerationController : Controller
    {

        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Leave_COFFGeneration
        #region ESS_Leave_COFFGeneration main 
        public ActionResult ESS_Leave_COFFGeneration(int? CmpId, string CoffGenerationID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]): 290;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                Atten_CoffGeneration AttenCoffGeneration = new Atten_CoffGeneration();
                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_CoffGeneration";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;

                if (CoffGenerationID_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_CoffGenerationID_Encrypted", CoffGenerationID_Encrypted);
                    AttenCoffGeneration = DapperORM.ReturnList<Atten_CoffGeneration>("sp_List_Atten_CoffGeneration", paramList).FirstOrDefault();
                    TempData["CoffGenerationDate"] = AttenCoffGeneration.CoffGenerationDate;
                    TempData["CoffExpriredDate"] = AttenCoffGeneration.CoffExpriredDate.ToString("yyyy-MM-dd");
                    TempData["ExtensionDate"] = AttenCoffGeneration.ExtensionDate.ToString("yyyy-MM-dd");
                    TempData["DocDate"] = AttenCoffGeneration.DocDate;

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", AttenCoffGeneration.CmpID);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.BranchName = BranchName;
                    var BranchId = BranchName[0].Id;

                    DynamicParameters paramEMP = new DynamicParameters();
                    paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name from Mas_Employee,Mas_Employee_Attendance where Mas_Employee.Deactivate=0  and Mas_Employee_Attendance.Deactivate=0 and Mas_Employee_Attendance.AttendanceEmployeeid=Mas_Employee.EmployeeId and CmpID=" + AttenCoffGeneration.CmpID + " and EmployeeBranchId= " + AttenCoffGeneration.CoffGenerationBranchID + " and Mas_Employee.EmployeeLeft=0 and Mas_Employee.EmployeeId<>1 and (EM_Atten_WOPH_CoffApplicable=1 or EM_Atten_CoffApplicable=1) order by Name");
                    var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                    ViewBag.EmployeeName = GetEmployeeName;

                    var GetDocNos = "Select Isnull((DocNo),0) As DocNo from Atten_CoffGeneration where CoffGenerationID_Encrypted='" + CoffGenerationID_Encrypted + "'";
                    var DocNos = DapperORM.DynamicQuerySingle(GetDocNos);
                    ViewBag.DocNo = DocNos;

                    DynamicParameters paramDays = new DynamicParameters();
                    paramDays.Add("@query", "select COFFGenerationDaysId as Id,COFFGenerationDay as Name from Atten_CoffGenerationDays where Deactivate=0 and CmpId=" + AttenCoffGeneration.CmpID + " and COFFGenerationDaysBranchId=" + AttenCoffGeneration.CoffGenerationBranchID + " order by Name");
                    ViewBag.GetCOFFGenerationDay = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramDays).ToList();
                }
                else
                {
                    ViewBag.BranchName = "";
                    ViewBag.EmployeeName = "";
                    ViewBag.GetCOFFGenerationDay = "";
                }

                return View(AttenCoffGeneration);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region CoffGenerationExists

        public ActionResult CoffGenerationExists(DateTime CoffGenerationDate, string CoffGenerationIDEncrypted, int? CoffGenerationEmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetCoffGenerationDate = CoffGenerationDate.ToString("yyyy-MM-dd");
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CoffGenerationDate", GetCoffGenerationDate);
                    param.Add("@p_CoffGenerationID_Encrypted", CoffGenerationIDEncrypted);
                    param.Add("@p_CoffGenerationEmployeeID", CoffGenerationEmployeeID);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_CoffGeneration", param);
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

        public ActionResult SaveUpdate(Atten_CoffGeneration CoffGeneration)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 290;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (CoffGeneration.CoffGenerationID_Encrypted != null)
                {
                    param.Add("@p_process", "Update");
                    param.Add("@p_CoffGenerationID_Encrypted", CoffGeneration.CoffGenerationID_Encrypted);
                    param.Add("@p_ExtensionDate", CoffGeneration.ExtensionDate);
                    param.Add("@p_ExtensionRemark", CoffGeneration.ExtensionRemark);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_Atten_CoffGeneration", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return RedirectToAction("ESS_Leave_COFFGeneration", "ESS_Leave_COFFGeneration");
                }
                else
                {
                    param.Add("@p_process", string.IsNullOrEmpty(CoffGeneration.CoffGenerationID_Encrypted) ? "Save" : "Update");
                    param.Add("@p_CmpID", CoffGeneration.CmpID);
                    param.Add("@p_CoffGenerationBranchID", CoffGeneration.CoffGenerationBranchID);
                    param.Add("@p_CoffGenerationEmployeeID", CoffGeneration.CoffGenerationEmployeeID);
                    param.Add("@p_CoffGenerationDate", CoffGeneration.CoffGenerationDate);
                    param.Add("@p_Approvenoofcoff", CoffGeneration.Approvenoofcoff);
                    param.Add("@p_DocNo", CoffGeneration.DocNo);
                    param.Add("@p_DocDate", CoffGeneration.DocDate);
                    param.Add("@p_RequestFrom", "CoffGeneration");
                    param.Add("@p_CoffGenerationManually", 1);
                    param.Add("@p_Reason", CoffGeneration.Reason);
                    param.Add("@p_ApproveRejectBy",Session["EmployeeId"]);
                    param.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_Atten_CoffGeneration", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return RedirectToAction("ESS_Leave_COFFGeneration", "ESS_Leave_COFFGeneration");
                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region GetList
        public ActionResult GetList(int? CmpId, int? CoffGenerationBranchID ,int? CoffGenerationEmployeeID, DateTime? FromDate,DateTime? ToDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 290;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                var CmpID = GetComapnyName[0].Id;

                //DynamicParameters paramBranch = new DynamicParameters();
                //paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                //paramBranch.Add("@p_CmpId", CmpID);
                //var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                ViewBag.BranchName = "";
               // var BranchId = BranchName[0].Id;

                //DynamicParameters paramEMP = new DynamicParameters();
                //paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name from Mas_Employee,Mas_Employee_Attendance where Mas_Employee.Deactivate=0  and Mas_Employee_Attendance.Deactivate=0 and Mas_Employee_Attendance.AttendanceEmployeeid=Mas_Employee.EmployeeId and CmpID=" + CmpID + " and EmployeeBranchId= " + BranchId + " and Mas_Employee.EmployeeLeft=0 and Mas_Employee.EmployeeId<>1 and (EM_Atten_WOPH_CoffApplicable=1 or EM_Atten_CoffApplicable=1) order by Name");
                //var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                //ViewBag.EmployeeName = GetEmployeeName;
                ViewBag.EmployeeName = "";

                if ((FromDate != null) && (ToDate != null))
                {
                    param.Add("@p_CoffGenerationID_Encrypted", "List");
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", CoffGenerationBranchID);
                    param.Add("@p_EmployeeId", CoffGenerationEmployeeID);
                    param.Add("@p_FromDate", FromDate);
                    param.Add("@p_ToDate", ToDate);
                    var data = DapperORM.DynamicList("sp_List_Atten_CoffGeneration", param);
                    ViewBag.GetCoffGeneration = data;
                }
                else
                {
                    ViewBag.GetCoffGeneration = "";
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

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployeeName

        public ActionResult GetEmployeeName(int Buninessunit, int companyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramEMP = new DynamicParameters();
                paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name from Mas_Employee,Mas_Employee_Attendance where Mas_Employee.Deactivate=0  and Mas_Employee_Attendance.Deactivate=0 and Mas_Employee_Attendance.AttendanceEmployeeid=Mas_Employee.EmployeeId and CmpID=" + companyId + " and EmployeeBranchId= " + Buninessunit + " and Mas_Employee.EmployeeLeft=0 and Mas_Employee.EmployeeId<>1 and (EM_Atten_WOPH_CoffApplicable=1 or EM_Atten_CoffApplicable=1) order by Name");
                var GetEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();

                DynamicParameters paramDays = new DynamicParameters();
                paramDays.Add("@query", "select COFFGenerationDaysId as Id,COFFGenerationDay as Name from Atten_CoffGenerationDays where Deactivate=0 and CmpId=" + companyId + " and COFFGenerationDaysBranchId=" + Buninessunit + " order by Name");
                var GetCOFFGenerationDay = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramDays).ToList();

                return Json(new { GetEmployee = GetEmployee, GetCOFFGenerationDay = GetCOFFGenerationDay }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        #endregion

        #region Delete

        public ActionResult Delete(string CoffGenerationID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 290;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_CoffGenerationID_Encrypted", CoffGenerationID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_CoffGeneration", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Leave_COFFGeneration");
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