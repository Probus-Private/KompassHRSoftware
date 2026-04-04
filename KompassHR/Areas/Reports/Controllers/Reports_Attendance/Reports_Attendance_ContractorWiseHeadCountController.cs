using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Reports_Attendance_ContractorWiseHeadCountController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Reports_Attendance_ContractorWiseHeadCount
        public ActionResult Reports_Attendance_ContractorWiseHeadCount(HeadCountSummary HeadCount)
        {
            try
            {
                if(Session["EmployeeId"]==null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;

                if(HeadCount.CmpId!=null)
                {
                    param = new DynamicParameters();
                    param.Add("@p_FromDate", HeadCount.FromDate);
                    var ContractorHeadCount = DapperORM.ReturnList<dynamic>("sp_Rpt_Atten_ConctractorWiseHeadCount", param).ToList();
                    ViewBag.ContractorWiseHeadCount = ContractorHeadCount;

                    param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", HeadCount.CmpId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.BusinessUnit = data;
                    TempData["ShowData"] = true;             

                }
                else
                {
                    ViewBag.BusinessUnit = "";
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            return View();
        }

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
    }
}