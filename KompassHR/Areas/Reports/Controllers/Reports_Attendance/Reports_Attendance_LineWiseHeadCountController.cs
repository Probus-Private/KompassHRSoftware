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
    public class Reports_Attendance_LineWiseHeadCountController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Reports_Attendance_LineWiseHeadCount
        public ActionResult Reports_Attendance_LineWiseHeadCount(HeadCountSummary HeadCount)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;

                if (HeadCount.CmpId != 0)
                {
                    param = new DynamicParameters();
                    param.Add("@p_FromDate", HeadCount.FromDate);
                    param.Add("@p_ToDate", HeadCount.ToDate);
                    //if (HeadCount.BranchId!=0)
                    //{
                    //    param.Add("@p_EmployeeBranchId", HeadCount.BranchId);
                    //    param.Add("@p_CmpId", HeadCount.CmpId);
                    //}
                    var LineHeadCount = DapperORM.ReturnList<dynamic>("sp_Rpt_atten_LineWiseHeadCount", param).ToList();
                    ViewBag.LineWiseHeadCount = LineHeadCount;

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