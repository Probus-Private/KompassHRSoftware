using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FNF;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_FNF
{
    public class ESS_FNF_FNFEmployeeDetailsController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        GetMenuList ClsGetMenuList = new GetMenuList();

        // GET: ESS/ESS_TMS_TMSReports
        public ActionResult ESS_FNF_FNFEmployeeDetails(FNF_Calculation FNF_Calculation)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 611;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_employeeid", Session["FNFEmployeeId"]);
                FNF_Calculation = DapperORM.ReturnList<FNF_Calculation>("sp_List_FNF_EmployeeResignationlist", param).FirstOrDefault();

                // ViewBag.GetEmployeeInfoList = data;
                TempData["CompanyName"] = FNF_Calculation.CompanyName;
                TempData["BusinessUnit"] = FNF_Calculation.BusinessUnit;
                TempData["Department"] = FNF_Calculation.departmentname;
                TempData["Designation"] = FNF_Calculation.designationname;
                TempData["Grade"] = FNF_Calculation.gradename;
                TempData["JoiningDate"] = FNF_Calculation.joiningDate;
                TempData["LastWorkingDate"] = FNF_Calculation.LastWorkingDate;
                TempData["WorkDuration"] = FNF_Calculation.WorkDuration;
                TempData["PrimaryMobile"] = FNF_Calculation.PrimaryMobile;
                TempData["AadhaarNo"] = FNF_Calculation.AadhaarNo;
                TempData["PAN"] = FNF_Calculation.PAN;
                TempData["PF_NO"] = FNF_Calculation.PF_NO;
                TempData["ESIC_NO"] = FNF_Calculation.ESIC_NO;
                TempData["PF_UAN"] = FNF_Calculation.PF_UAN;
                TempData["ResignationDate"] = FNF_Calculation.ResignationDate;
                TempData["ReasonName"] = FNF_Calculation.ReasonName;
                TempData["ApproveRejectBy"] = FNF_Calculation.ApproveRejectBy;
                TempData["ApproveRejectRemark"] = FNF_Calculation.ApproveRejectRemark;
                TempData["ApproveRejectDate"] = FNF_Calculation.ApproveRejectDate;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #region Partial View 
        [HttpGet]
        public PartialViewResult FNF_SidebarMenu()
        {
            try
            {

                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", OnboardEmployeeId);
                var StatusCheck = DapperORM.DynamicList("sp_List_Mas_Employee_StatusCheck", paramList);
                ViewBag.GetStatusCheckList = StatusCheck;

                var id = Session["ModuleId"];
                var ScreenId = Session["ScreenId"];
                var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(id), Convert.ToInt32(ScreenId), "SubForm", "Transation");
                ViewBag.GetUserMenuList = GetMenuList;

                return PartialView("_FNFSidebarMenu");
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}