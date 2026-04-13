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
    public class Reports_Attendance_DepartmentWiseHeadCountController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Reports_Attendance_DepartmentWiseHeadCount
        public ActionResult Reports_Attendance_DepartmentWiseHeadCount(HeadCountSummary HeadCount)
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
                    //param.Add("@p_ToDate", HeadCount.ToDate);
                    if (HeadCount.BranchId != 0)
                    {
                        param.Add("@p_EmployeeBranchId", HeadCount.BranchId);
                        //param.Add("@p_CmpId", HeadCount.CmpId);
                    }
                    else
                    {
                      
                        var GetBranchId = DapperORM.DynamicQuerySingle(@"select mas_branch.BranchID from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + HeadCount.CmpId + " and UserBranchMapping.IsActive = 1").ToList(); ;
                        //DynamicParameters paramEMP = new DynamicParameters();
                        //paramEMP.Add("@query", "select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + HeadCount.CmpId + " and UserBranchMapping.IsActive = 1");
                        //var GetBranchId1 = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramEMP).ToList();
                        //param.Add("@p_EmployeeBranchId", GetBranchId.Id);
                        //for (int i = 0; i < GetBranchId1.Count(); i++)
                        //{
                        //    number[i] = DapperORM.GetBranchId.Id;
                        //}
                        var count = GetBranchId.Count();
                        int i = 0;
                        decimal[] number=new decimal[count];
                        ViewBag.BranchId = GetBranchId;
                        foreach (var d in ViewBag.BranchId)
                            {

                            number[i] = d.BranchID;
                                i++;
                            }
                        param.Add("@p_EmployeeBranchId", number);
                    }

                    var DepartmentHeadCount = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_DepartmentWiseDailyHeadCount", param).ToList();
                    ViewBag.DepartmentWiseHeadCount = DepartmentHeadCount;


                    param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", HeadCount.CmpId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.BusinessUnit = data;
                    TempData["ShowData"] = true;
                    //var Column = ViewBag.ContractorWiseHeadCount.Keys(Data[0]);


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